﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReinforcementLearning
{
    enum Move
    {
        Up,
        Down,
        Left,
        Right,
    }

    public class Learn
    {
        private Point _mapSize = new(4, 4);
        private Point _destination = new(4, 4);
        private Point _current; //current coor
        private (int? min, int score) _scoreData = (null, 10); //weight score
        private Point[] _obstacle = [new(3, 3), new(4, 3)]; //obstacle coor
        private List<(Point, Move)> _record = []; //record path taken
        private Dictionary<Point, Dictionary<Move, int?>> _qValue = []; //all weight by coor by direction

        public void Run()
        {
            int repeatCount = 0;
            while (true)
            {
                repeatCount++;
                _record.Clear();
                _current = new(1, 1);

                while (true)
                {
                    var next = NextMove();
                    _record.Add((_current, next));

                    //set current coor
                    (next switch
                    {
                        Move.Up => (Action)(() => { _current.Y--; }),
                        Move.Down => (Action)(() => { _current.Y++; }),
                        Move.Left => (Action)(() => { _current.X--; }),
                        Move.Right => (Action)(() => { _current.X++; }),
                        _ => throw new Exception()
                    })();

                    //arrive
                    if (_current.X == _destination.X && _current.Y == _destination.Y)
                    {
                        var score = 10; //weight score

                        if (_scoreData.min == null) //first arrive
                        {
                            _scoreData.min = _record.Count;
                        }
                        else
                        {
                            if (_scoreData.min > _record.Count) //short record
                            {
                                _scoreData.score += (_scoreData.min - _record.Count).Value; //plus shortened record
                                _scoreData.min = _record.Count; //set min record

                                score = _scoreData.score;
                            }
                            else if (_scoreData.min == _record.Count) //same record
                            {
                                _scoreData.score += 10; //same record is plus weight

                                score = _scoreData.score;
                            }
                        }

                        foreach (var recordData in _record)
                        {
                            _qValue.TryGetValue(recordData.Item1, out var dic);
                            if (dic == null)
                            {
                                dic = [];
                                dic.Add(recordData.Item2, score);
                            }
                            else
                            {
                                dic.TryGetValue(recordData.Item2, out var value);
                                if (value == null)
                                {
                                    dic.Add(recordData.Item2, score);
                                }
                                else //already weight is average
                                {
                                    dic[recordData.Item2] = (score + value) / 2;
                                }
                            }
                            _qValue.TryAdd(recordData.Item1, dic);
                        }

                        break;
                    }
                }
            }
        }

        private Move NextMove()
        {
            while (true)
            {
                _qValue.TryGetValue(_current, out var dic); //get move weights from current coor

                Dictionary<Move, int> scoreDic = [];

                if (dic == null) //no data is same weight
                {
                    scoreDic.Add(Move.Up, 1);
                    scoreDic.Add(Move.Down, 1);
                    scoreDic.Add(Move.Left, 1);
                    scoreDic.Add(Move.Right, 1);
                }
                else
                {
                    int weightSum = 0, emptyCount = 0;
                    Dictionary<Move, int?> tempDic = []; //move weights datas

                    foreach (var key in Enum.GetValues<Move>()) //qValue weight
                    {
                        dic.TryGetValue(key, out var value); //get weight from move
                        if (value != null)
                        {
                            weightSum += value.Value;
                            emptyCount++;
                            tempDic.Add(key, value);
                        }
                        else
                        {
                            tempDic.Add(key, null);
                        }
                    }
                    foreach (var key in Enum.GetValues<Move>()) //set empty move weights
                    {
                        if (tempDic[key] == null)
                        {
                            tempDic[key] = weightSum / emptyCount;
                        }
                    }
                    foreach (var key in Enum.GetValues<Move>()) //set scoreDic
                    {
                        scoreDic.Add(key, tempDic[key]!.Value);
                    }
                }

                //get next move by weight
                Move? move = null;
                int weight = 0;
                var select = new Random().Next(scoreDic.Values.Sum());
                foreach (var key in scoreDic.Keys)
                {
                    weight += scoreDic[key];
                    if (select < weight)
                    {
                        move = key;
                        break;
                    }
                }

                if (move == null)
                {
                    throw new Exception("fail get next move");
                }

                //check map outside
                if ((_current.X == 1 && move == Move.Left) ||
                    (_current.Y == 1 && move == Move.Up) ||
                    (_current.X == _mapSize.X && move == Move.Right) ||
                    (_current.Y == _mapSize.Y && move == Move.Down))
                {
                    continue;
                }

                //check obstacle
                Point p = move switch
                {
                    Move.Up => new Point(_current.X, _current.Y - 1),
                    Move.Down => new Point(_current.X, _current.Y + 1),
                    Move.Left => new Point(_current.X - 1, _current.Y),
                    Move.Right => new Point(_current.X + 1, _current.Y),
                    _ => throw new Exception()
                };
                if (_obstacle.Contains(p))
                    continue;

                return move.Value;
            }
        }



    }
}
