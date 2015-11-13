﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;

namespace Joueur.cs
{
    class Program
    {
        static void Main(string[] args)
        {
            var argParser = new ArgParser(args, "Runs the C# client with options. Must a provide a game name to play on the server.", new ArgParser.Argument[] {
                new ArgParser.Argument(new string[] {"game"}, "game", "the name of the game you want to play on the server", true),
                new ArgParser.Argument(new string[] {"-s", "--server"}, "server", "the url to the server you want to connect to e.g. locahost:3000", false, "127.0.0.1"),
                new ArgParser.Argument(new string[] {"-p", "--port"}, "port", "the port to connect to on the server. Can be defined on the server arg via server:port", false, 3000),
                new ArgParser.Argument(new string[] {"-n", "--name"}, "name", "the name you want to use as your AI\'s player name. This over-rides the name you set in your code"),
                new ArgParser.Argument(new string[] {"-r", "--session"}, "requestedSession", "the requested game session you want to play on the server", false, "*"),
                new ArgParser.Argument(new string[] {"-w", "--password"}, "password", "the password required for authentication on official servers"),
                new ArgParser.Argument(new string[] {"--gameSettings"}, "gameSettings", "Any settings for the game server to force. Must be url parms formatted (key=value&otherKey=otherValue)"),
                new ArgParser.Argument(new string[] {"--printIO"}, "printIO", "(debugging) print IO through the TCP socket to the terminal", false, null, ArgParser.Argument.Store.True),
            }, (int)ErrorHandler.ErrorCode.INVALID_ARGS);

            string gameName = argParser.GetValue<string>("game");
            string server = argParser.GetValue<string>("server");
            string playerName = argParser.GetValue<string>("name");
            string requestedSession = argParser.GetValue<string>("requestedSession");
            string password = argParser.GetValue<string>("password");
            string gameSettings = argParser.GetValue<string>("gameSettings");
            int port = argParser.GetValue<int>("port");
            bool printIO = argParser.GetValue<bool>("printIO");

            if (server.Contains(":"))
            {
                var split = server.Split(':');
                server = split[0];
                port = Int32.Parse(split[1]);
            }

            BaseGame game = null;
            BaseAI ai = null;

            try
            {
                Type gameType = Type.GetType("Joueur.cs.Games." + gameName + ".Game");
                game = (BaseGame)Activator.CreateInstance(gameType, true);

                Type aiType = Type.GetType("Joueur.cs.Games." + gameName + ".AI");
                ai = (BaseAI)Activator.CreateInstance(aiType, true);
            }
            catch(Exception exception)
            {
                ErrorHandler.HandleError(ErrorHandler.ErrorCode.GAME_NOT_FOUND, exception, "Could create Game and AI for game name '" + gameName + "'");
            }

            Client client = Client.Instance;

            client.ConnectTo(game, ai, server, port, printIO);

            if (String.IsNullOrWhiteSpace(playerName))
            {
                playerName = ai.GetName();
            }

            client.Send("play", new ServerMessages.SendPlay
                {
                    playerName = playerName,
                    gameName = gameName,
                    password = password,
                    gameSettings = gameSettings,
                    requestedSession = requestedSession
                }
            );

            var lobbiedData = (ServerMessages.LobbiedData)client.WaitForEvent("lobbied");

            Console.WriteLine("In Lobby for game '" + lobbiedData.gameName + "' in session '" + lobbiedData.gameSession + "'.");


            // hackish way to set the client in the game. we don't want to expose public methods that competitors may see via intellisense and try to use
            client.GameManager.SetConstants(lobbiedData.constants);

            var startData = (ServerMessages.StartData)client.WaitForEvent("start");
            
            Console.WriteLine("Game starting");

            // set the AI's game and player via reflection
            try
            {
                ai.GetType().GetField("Game").SetValue(ai, game);
                ai.GetType().GetField("Player").SetValue(ai, game.GameObjects[startData.playerID]);
            }
            catch(Exception exception)
            {
                ErrorHandler.HandleError(ErrorHandler.ErrorCode.REFLECTION_FAILED, exception, "Could not set the Game and Player for the AI during game startup.");
            }

            try
            {
                ai.Start();
                ai.GameUpdated();
            }
            catch(Exception exception)
            {
                ErrorHandler.HandleError(ErrorHandler.ErrorCode.AI_ERRORED, exception, "AI errored during initial game start.");
            }

            client.Play();
        }
    }
}
