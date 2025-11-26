using System.Threading;
using Helteix.Tools.Phases;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ATCG.GameModes
{
    public class WaitForEveryPlayerPhase : IPhase<bool>
    {
        public const string SESSION_WAIT_STATE = "WaitState";
        public const string PLAYER_IS_READY = "IsReady";
        public ISession Session { get; }

        public bool WantsToLeave { get; }

        public WaitForEveryPlayerPhase(ISession session)
        {
            Session = session;
        }

        async Awaitable IPhase<bool>.Initialize(CancellationToken token)
        {
            Session.Deleted += this.Cancel;
            if (Session is IHostSession hostSession)
            {
                hostSession.SetProperty(SESSION_WAIT_STATE, new SessionProperty(bool.FalseString, VisibilityPropertyOptions.Member));
                await hostSession.SavePropertiesAsync();
            }


            Session.CurrentPlayer.SetProperty(PLAYER_IS_READY, new PlayerProperty(bool.FalseString, VisibilityPropertyOptions.Member));
            await Session.SaveCurrentPlayerDataAsync();
        }


        async Awaitable IPhase<bool>.Dispose(CancellationToken token)
        {
            Session.Deleted -= this.Cancel;
            if (token.IsCancellationRequested)
            {
                await Session.LeaveAsync();

                token.ThrowIfCancellationRequested();
                return;
            }

            if (Session is IHostSession hostSession && hostSession.Properties.ContainsKey(SESSION_WAIT_STATE))
            {
                hostSession.SetProperty(SESSION_WAIT_STATE, null);
                await hostSession.SavePropertiesAsync();
            }

            Session.CurrentPlayer.SetProperty(PLAYER_IS_READY, null);
            await Session.SaveCurrentPlayerDataAsync();
        }


        async Awaitable<bool> IPhase<bool>.Execute(CancellationToken token)
        {
            IHostSession hostSession = Session as IHostSession;
            bool isHost = hostSession != null;

            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (Session.Properties.TryGetValue(SESSION_WAIT_STATE, out SessionProperty waitState))
                {
                    if(bool.TryParse(waitState.Value, out bool result) && result)
                        break;
                }

                if (isHost)
                {
                    bool areAllPlayerReady = true;
                    foreach (IReadOnlyPlayer player in Session.Players)
                    {
                        if (player.Properties.TryGetValue(PLAYER_IS_READY, out PlayerProperty playerProperty)
                            && playerProperty.Value == bool.TrueString)
                            continue;

                        areAllPlayerReady = false;
                        break;
                    }

                    string value = areAllPlayerReady.ToString();
                    if(hostSession.Properties[SESSION_WAIT_STATE].Value != value)
                    {
                        hostSession.SetProperty(SESSION_WAIT_STATE, new SessionProperty(value, VisibilityPropertyOptions.Member));
                        await hostSession.SavePropertiesAsync();
                    }

                }

                await Awaitable.NextFrameAsync(token);
            }

            if (isHost)
            {
                hostSession.SetProperty(SESSION_WAIT_STATE, null);
                await Session.SaveCurrentPlayerDataAsync();
            }
            else
            {
                while (Session.Properties.ContainsKey(SESSION_WAIT_STATE))
                {
                    token.ThrowIfCancellationRequested();
                    await Awaitable.NextFrameAsync(token);
                }
            }

            return true;
        }


        public void Leave() => this.Cancel();

        public void SetReadyState(bool value)
        {
            Session.CurrentPlayer.SetProperty(PLAYER_IS_READY, new PlayerProperty(value.ToString(), VisibilityPropertyOptions.Member));
            Session.SaveCurrentPlayerDataAsync();
        }

    }
}