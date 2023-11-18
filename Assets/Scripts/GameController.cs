using System;
using System.Collections;
using OPS.AntiCheat.Field;
using UnityEngine;
using Watermelon_Game.Container;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus;

namespace Watermelon_Game
{
    /// <summary>
    /// Contains general game and game-state logic
    /// </summary>
    internal sealed class GameController : MonoBehaviour
    {
        #region Fields
        /// <summary>
        /// <see cref="ResetReason"/>
        /// </summary>
        private ResetReason resetReason;
        #endregion
        
        #region Properties
        /// <summary>
        /// Indicates whether the game is over or running
        /// </summary>
        public static bool IsGameRunning { get; private set; }
        /// <summary>
        /// Timestamp in seconds, when the currently active game was started -> <see cref="Time"/>.<see cref="Time.time"/> <br/>
        /// <i>Is reset on every <see cref="GameController"/>.<see cref="GameController.StartGame"/></i>
        /// </summary>
        public static ProtectedFloat CurrentGameTimeStamp { get; private set; }
        /// <summary>
        /// Will be true when the application is about to be closed
        /// </summary>
        public static bool IsApplicationQuitting { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Is called every time a game starts
        /// </summary>
        public static event Action OnGameStart;
        /// <summary>
        /// Is called when the game is being reset -> <see cref="ResetGame"/>
        /// </summary>
        public static event Action OnResetGameStarted;
        /// <summary>
        /// Is called when <see cref="ResetGame"/> has finished
        /// </summary>
        public static event Action OnResetGameFinished;
        /// <summary>
        /// Is called <see cref="MaxHeight.OnGameOver"/> after <see cref="ResetGame"/> has finished
        /// </summary>
        public static event Action OnRestartGame;
        #endregion
        
        #region Methods
        private void Awake()
        {
            Application.targetFrameRate = 120;
        }

        private void OnEnable()
        {
            MaxHeight.OnGameOver += this.GameOver;
            MenuController.OnManualRestart += this.ManualRestart;
            OnResetGameFinished += this.GameReset;
            GameOverMenu.OnGameOverMenuClosed += StartGame;
            Application.quitting += this.ApplicationIsQuitting;
        }

        private void OnDisable()
        {
            MaxHeight.OnGameOver -= this.GameOver;
            MenuController.OnManualRestart -= this.ManualRestart;
            OnResetGameFinished -= this.GameReset;
            GameOverMenu.OnGameOverMenuClosed -= StartGame;
            Application.quitting -= this.ApplicationIsQuitting;
        }
        
        private void Start()
        {
            // TODO: Replace with menu logic
            //StartGame(); // TODO
        }
        
        /// <summary>
        /// Starts the game
        /// </summary>
        public static void StartGame() // TODO: Make private
        {
            IsGameRunning = true;
            OnGameStart?.Invoke();
            CurrentGameTimeStamp = Time.time;
        }
        
        /// <summary>
        /// <see cref="MaxHeight.OnGameOver"/>
        /// </summary>
        private void GameOver()
        {
            IsGameRunning = false;
            this.resetReason = ResetReason.GameOver;
            base.StartCoroutine(ResetGame());
        }

        /// <summary>
        /// <see cref="MenuController.OnManualRestart"/> 
        /// </summary>
        private void ManualRestart()
        {
            this.resetReason = ResetReason.ManualRestart;
            base.StartCoroutine(ResetGame());
        }
        
        /// <summary>
        /// Resets the game to its initial state
        /// </summary>
        /// <returns></returns>
        private IEnumerator ResetGame()
        {
            OnResetGameStarted?.Invoke();
            
            var _waitTime = new WaitForSeconds(.1f);
            var _fruits = FruitController.Fruits;
            
            // ReSharper disable once InconsistentNaming
            for (var i = _fruits.Count - 1; i >= 0; i--)
            {
                if (_fruits[i] != null) // TODO: Null check shouldn't be necessary, but fruit is sometimes null for some reason
                {
                    _fruits[i].DestroyFruit();   
                }
                
                yield return _waitTime;
            }
            
            OnResetGameFinished?.Invoke();
        }

        /// <summary>
        /// <see cref="OnResetGameFinished"/>
        /// </summary>
        private void GameReset()
        {
            switch (this.resetReason)
            {
                case ResetReason.GameOver:
                    OnRestartGame?.Invoke();
                    break;
                case ResetReason.ManualRestart:
                    StartGame();
                    break;
            }
        }
        
        /// <summary>
        /// <see cref="Application.quitting"/>
        /// </summary>
        private void ApplicationIsQuitting()
        {
            IsApplicationQuitting = true;
        }
        #endregion
    }
}