using Common;
using Common.Message;
using Common.Config;
using CommunityToolkit.Mvvm.Messaging;
using ML;

namespace Sequence
{
    /// <summary>
    /// flow program sequence
    /// </summary>
    public class Flow : IRecipient<MainWindowRenderedMessage>, IRecipient<MainViewCloseMessage>
    {
        private readonly Log _log;
        private readonly Execute _execute;

        public Flow(Log log, Execute execute)
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
            _log = log;
            _execute = execute;
        }

        public async void Receive(MainWindowRenderedMessage message)
        {
            try
            {
                //do init
                SettingData config = new();
                config.Load();
                await _execute.LoadAsync(config.ModelName);
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new DialogMessage("init error", ex.Message));
                _log.Write(ex.Message);
            }
            finally
            {
                WeakReferenceMessenger.Default.Send(new BusyMessage(false)); //close wait
            }
        }

        public async void Receive(MainViewCloseMessage message)
        {
            WeakReferenceMessenger.Default.Send(new BusyMessage(true, "exit..."));
            try
            {
                //do dispose
                await _execute.DisposeAsync();

                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new DialogMessage("dispose error", ex.Message));
                _log.Write(ex.Message);
            }
        }
    }
}
