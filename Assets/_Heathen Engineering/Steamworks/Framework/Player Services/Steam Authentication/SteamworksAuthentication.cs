#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    public static class SteamworksAuthentication
    {
        /// <summary>
        /// Tickets this player has sent out
        /// </summary>
        public static List<Ticket> ActiveTickets;
        /// <summary>
        /// Sessions this player has started
        /// </summary>
        public static List<Session> ActiveSessions;

        private static Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponce;
        private static Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponceServer;
        private static Callback<ValidateAuthTicketResponse_t> m_ValidateAuthSessionTicketResponce;
        private static Callback<ValidateAuthTicketResponse_t> m_ValidateAuthSessionTicketResponceServer;

        private static bool callbacksRegistered;

        /// <summary>
        /// Registers the callbacks for the system ... 
        /// this automatically gets called with any operation but can be called manually if needed
        /// </summary>
        /// <returns></returns>
        public static bool RegisterCallbacks()
        {
            if (SteamSettings.current.Initialized)
            {
                if (!callbacksRegistered)
                {
                    callbacksRegistered = true;
                    m_GetAuthSessionTicketResponce = Callback<GetAuthSessionTicketResponse_t>.Create(HandleGetAuthSessionTicketResponce);
                    m_GetAuthSessionTicketResponceServer = Callback<GetAuthSessionTicketResponse_t>.CreateGameServer(HandleGetAuthSessionTicketResponce);
                    m_ValidateAuthSessionTicketResponce = Callback<ValidateAuthTicketResponse_t>.Create(HandleValidateAuthTicketResponse);
                    m_ValidateAuthSessionTicketResponceServer = Callback<ValidateAuthTicketResponse_t>.CreateGameServer(HandleValidateAuthTicketResponse);
                    return true;
                }
                else
                    return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Determins if the provided ticket handle is valid
        /// </summary>
        /// <param name="ticket">the ticket to test for validity</param>
        /// <returns></returns>
        public static bool IsAuthTicketValid(Ticket ticket)
        {
            RegisterCallbacks();

            if (ticket.Handle == default || ticket.Handle == HAuthTicket.Invalid)
                return false;
            else
                return true;
        }

        /// <summary>
        /// <para>Encodes a ticekt to hex string format</para>
        /// This is most commonly used with web calls such as <a href="https://partner.steamgames.com/doc/webapi/ISteamUserAuth#AuthenticateUserTicket">https://partner.steamgames.com/doc/webapi/ISteamUserAuth#AuthenticateUserTicket</a>
        /// </summary>
        /// <param name="ticket">The ticket to be encoded</param>
        /// <returns>Returns the hex encoded string representation of the ticket data array.</returns>
        public static string EncodedAuthTicket(Ticket ticket)
        {
            RegisterCallbacks();

            if (!IsAuthTicketValid(ticket))
                return "";
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in ticket.Data)
                    sb.AppendFormat("{0:X2}", b);

                return sb.ToString();
            }
        }

        /// <summary>
        /// <para>Retrieve a authentication ticket to be sent to the entity who wishes to authenticate you.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUser#GetAuthSessionTicket">https://partner.steamgames.com/doc/api/ISteamUser#GetAuthSessionTicket</a>
        /// </summary>
        /// <returns></returns>
        public static Ticket ClientGetAuthSessionTicket()
        {
            RegisterCallbacks();

            uint m_pcbTicket;
            var ticket = new Ticket();
            ticket.isClientTicket = true;
            ticket.Data = new byte[1024];
            ticket.Handle = SteamUser.GetAuthSessionTicket(ticket.Data, 1024, out m_pcbTicket);
            ticket.CreatedOn = SteamUtils.GetServerRealTime();
            Array.Resize(ref ticket.Data, (int)m_pcbTicket);

            if (ActiveTickets == null)
                ActiveTickets = new List<Ticket>();

            ActiveTickets.Add(ticket);

            return ticket;
        }

        /// <summary>
        /// <para>Retrieve a authentication ticket to be sent to the entity who wishes to authenticate you.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamGameServer#GetAuthSessionTicket">https://partner.steamgames.com/doc/api/ISteamGameServer#GetAuthSessionTicket</a>
        /// </summary>
        /// <returns></returns>
        public static Ticket ServerGetAuthSessionTicket()
        {
            RegisterCallbacks();

            uint m_pcbTicket;
            var ticket = new Ticket();
            ticket.isClientTicket = false;
            ticket.Data = new byte[1024];
            ticket.Handle = SteamGameServer.GetAuthSessionTicket(ticket.Data, 1024, out m_pcbTicket);
            ticket.CreatedOn = SteamUtils.GetServerRealTime();
            Array.Resize(ref ticket.Data, (int)m_pcbTicket);

            if (ActiveTickets == null)
                ActiveTickets = new List<Ticket>();

            ActiveTickets.Add(ticket);

            return ticket;
        }

        /// <summary>
        /// Cancels the auth ticket rather its client or server based.
        /// </summary>
        /// <param name="ticket"></param>
        public static void CancelAuthTicket(Ticket ticket)
        {
            RegisterCallbacks();

            ticket.Cancel();
        }

        /// <summary>
        /// <para>Authenticate the ticket from the entity Steam ID to be sure it is valid and isn't reused.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUser#BeginAuthSession">https://partner.steamgames.com/doc/api/ISteamUser#BeginAuthSession</a>
        /// </summary>
        /// <param name="authTicket"></param>
        /// <param name="user"></param>
        /// <param name="callback"></param>
        public static void ClientBeginAuthSession(byte[] authTicket, CSteamID user, Action<Session> callback)
        {
            RegisterCallbacks();

            var session = new Session()
            {
                isClientSession = true,
                User = user,
                OnStartCallback = callback
            };

            if(ActiveSessions == null)
            {
                ActiveSessions = new List<Session>();
            }
            ActiveSessions.Add(session);

            SteamUser.BeginAuthSession(authTicket, authTicket.Length, user);
        }

        /// <summary>
        /// <para>Authenticate the ticket from the entity Steam ID to be sure it is valid and isn't reused.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamGameServer#BeginAuthSession">https://partner.steamgames.com/doc/api/ISteamGameServer#BeginAuthSession</a>
        /// </summary>
        /// <param name="authTicket"></param>
        /// <param name="user"></param>
        /// <param name="callback"></param>
        public static void ServerBeginAuthSession(byte[] authTicket, CSteamID user, Action<Session> callback)
        {
            RegisterCallbacks();

            var session = new Session()
            {
                isClientSession = false,
                User = user,
                OnStartCallback = callback
            };

            if (ActiveSessions == null)
            {
                ActiveSessions = new List<Session>();
            }
            ActiveSessions.Add(session);

            SteamGameServer.BeginAuthSession(authTicket, authTicket.Length, user);
        }

        /// <summary>
        /// <para>Ends an auth session that was started with BeginAuthSession. This should be called when no longer playing with the specified entity.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUser#EndAuthSession">https://partner.steamgames.com/doc/api/ISteamUser#EndAuthSession</a>
        /// </summary>
        /// <param name="user"></param>
        public static void ClientEndAuthSession(CSteamID user)
        {
            SteamUser.EndAuthSession(user);
        }

        /// <summary>
        /// <para>Ends an auth session that was started with BeginAuthSession. This should be called when no longer playing with the specified entity.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamGameServer#EndAuthSession">https://partner.steamgames.com/doc/api/ISteamGameServer#EndAuthSession</a>
        /// </summary>
        /// <param name="user"></param>
        public static void ServerEndAuthSession(CSteamID user)
        {
            SteamGameServer.EndAuthSession(user);
        }

        private static void HandleGetAuthSessionTicketResponce(GetAuthSessionTicketResponse_t pCallback)
        {
            if (ActiveTickets != null && ActiveTickets.Any(p => p.Handle == pCallback.m_hAuthTicket))
            {
                var ticket = ActiveTickets.First(p => p.Handle == pCallback.m_hAuthTicket);

                if (ticket.Handle != default(HAuthTicket) && ticket.Handle != HAuthTicket.Invalid
                    && pCallback.m_eResult == EResult.k_EResultOK)
                    ticket.Verified = true;
            }
        }

        private static void HandleValidateAuthTicketResponse(ValidateAuthTicketResponse_t param)
        {
            if(ActiveSessions != null && ActiveSessions.Any(p => p.User == param.m_SteamID))
            {
                var session = ActiveSessions.First(p => p.User == param.m_SteamID);
                session.Responce = param.m_eAuthSessionResponse;
                session.GameOwner = param.m_OwnerSteamID;

                Debug.Log("Processing session request data for " + param.m_SteamID.m_SteamID.ToString() + " status = " + param.m_eAuthSessionResponse);

                if (session.OnStartCallback != null)
                    session.OnStartCallback.Invoke(session);
            }
            else
            {
                Debug.LogWarning("Recieved an authentication ticket responce for user " + param.m_SteamID.m_SteamID + " no matching session was found for this user.");
            }
        }

        /// <summary>
        /// Ends all tracked sessions
        /// </summary>
        public static void EndAllSessions()
        {
            foreach (var session in ActiveSessions)
                session.End();
        }

        /// <summary>
        /// Cancels all tracked tickets
        /// </summary>
        public static void CancelAllTickets()
        {
            foreach (var ticket in ActiveTickets)
                ticket.Cancel();
        }

        /// <summary>
        /// Represents an authentication session and carries unique information about the session request such as the user the session is inrealation to and the ticket data of the session.
        /// </summary>
        [Serializable]
        public class Session
        {
            /// <summary>
            /// Indicates that this session is being managed by a client or server
            /// </summary>
            public bool isClientSession = true;
            /// <summary>
            /// The user this session is in relation to
            /// </summary>
            public CSteamID User;
            /// <summary>
            /// The ID of the user that owns the game the user of this session is playing ... e.g. if this differes from the user then this is a barrowed game.
            /// </summary>
            public CSteamID GameOwner;
            /// <summary>
            /// The session data aka the 'ticket' data.
            /// </summary>
            public byte[] Data;
            /// <summary>
            /// The responce recieved when validating a provided ticket.
            /// </summary>
            public EAuthSessionResponse Responce;
            /// <summary>
            /// If true then the game this user is playing is barrowed from another user e.g. this user does not have a license for this game but is barrowing it from another user.
            /// </summary>
            public bool IsBarrowed { get { return User != GameOwner; } }
            /// <summary>
            /// The callback deligate to be called when the authenticate session responce returns from the steam backend.
            /// </summary>
            public Action<Session> OnStartCallback;

            /// <summary>
            /// Ends the authentication session.
            /// </summary>
            public void End()
            {
                if (isClientSession)
                    SteamUser.EndAuthSession(User);
                else
                    SteamGameServer.EndAuthSession(User);
            }
        }

        /// <summary>
        /// Represents a ticekt such as is generated by a user and sent to start an authentication session.
        /// </summary>
        [Serializable]
        public class Ticket
        {
            /// <summary>
            /// Indicates that this session is being managed by a client or server
            /// </summary>
            public bool isClientTicket = true;
            /// <summary>
            /// The authentication handle assoceated with this ticket
            /// </summary>
            public HAuthTicket Handle;
            /// <summary>
            /// The ticket data of this ticket ... this is what should be sent to servers for processing
            /// </summary>
            public byte[] Data;
            /// <summary>
            /// Has this ticket been verified, this gets set to true when the Get Authentication Session responce comes back from the Steam backend.
            /// </summary>
            public bool Verified;
            /// <summary>
            /// The Steam date time this ticket was created
            /// </summary>
            public uint CreatedOn;

            /// <summary>
            /// The age of this ticket from the current server realtime
            /// </summary>
            public TimeSpan Age
            {
                get { return new TimeSpan(0, 0, (int)(SteamUtils.GetServerRealTime() - CreatedOn)); }
            }

            /// <summary>
            /// Cancels the ticekt
            /// </summary>
            public void Cancel()
            {
                if (isClientTicket)
                    SteamUser.CancelAuthTicket(Handle);
                else
                    SteamGameServer.CancelAuthTicket(Handle);
            }
        }
    }
}
#endif