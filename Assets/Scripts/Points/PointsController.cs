using System;
using System.Collections;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using TMPro;
using UnityEngine;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus.MenuContainers;
using Watermelon_Game.Singletons;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Points
{
    internal sealed class PointsController : PersistantGameModeTransition<PointsController>
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the Multiplier component")]
        [SerializeField] private Multiplier multiplier;
        [Tooltip("Reference to the TMP component that displays the current points")]
        [SerializeField] private TextMeshProUGUI pointsAmount;
        [Tooltip("Reference to the TMP component that displays the best score")]
        [SerializeField] private TextMeshProUGUI bestScoreAmount;
        
        [Header("Settings")]
        [Tooltip("Time in seconds to wait, between each update of \"pointsAmount\", when the points change")]
        [SerializeField] private float pointsWaitTime = .05f;
        #endregion
        
        #region Fields
        /// <summary>
        /// The current points amount <br/>
        /// <i>Will be reset on <see cref="GameController"/><see cref="GameController.OnResetGameFinished"/></i>
        /// </summary>
        private ProtectedUInt32 currentPoints;
        /// <summary>
        /// Points that need to be added/subtracted from <see cref="currentPoints"/> (If != 0)
        /// </summary>
        private ProtectedUInt32 pointsDelta;
        
        /// <summary>
        /// Adds/subtract the amount in <see cref="pointsDelta"/> from <see cref="currentPoints"/>
        /// </summary>
        [CanBeNull] private IEnumerator pointsCoroutine;
        /// <summary>
        /// <see cref="pointsWaitTime"/>
        /// </summary>
        private WaitForSeconds pointsWaitForSeconds;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="currentPoints"/>
        /// </summary>
        public static ProtectedUInt32 CurrentPoints => Instance.currentPoints;
        #endregion

        #region Events
        /// <summary>
        /// Is called when <see cref="currentPoints"/> value changes <br/>
        /// <b>Parameter:</b> The value of <see cref="currentPoints"/>
        /// </summary>
        public static event Action<uint> OnPointsChanged; 
        #endregion
        
        #region Methods
        protected override void Init()
        {
            base.Init();
            this.pointsWaitForSeconds = new WaitForSeconds(this.pointsWaitTime);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            GameController.OnResetGameFinished += this.ResetPoints;
            FruitController.OnEvolve += AddPoints;
            FruitController.OnGoldenFruitCollision += AddPoints;
            SkillController.OnSkillUsed += this.SubtractPoints;
            MenuContainer.OnNewBestScore += this.NewBestScore;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            GameController.OnResetGameFinished -= this.ResetPoints;
            FruitController.OnEvolve -= AddPoints;
            FruitController.OnGoldenFruitCollision += AddPoints;
            SkillController.OnSkillUsed -= this.SubtractPoints;
            MenuContainer.OnNewBestScore -= this.NewBestScore;
        }

        private void Start()
        {
            this.bestScoreAmount.text = GlobalStats.Instance.BestScore.ToString();
        }
        
        /// <summary>
        /// Adds points to <see cref="currentPoints"/> depending on the given <see cref="Fruit"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to get the points for</param>
        private void AddPoints(Fruit _Fruit)
        {
            // TODO: Dirty fix
            // TODO: When a client is connected to a host, this is null when the clients golden fruit collides with another fruit
            if (this.multiplier == null)
            {
                return;
            }
            
            this.multiplier.StartMultiplier();

            var _points = (int)((int)_Fruit + this.multiplier.CurrentMultiplier);
            this.SetPoints(_points);
        }
        
        /// <summary>
        /// Subtracts the given amount from <see cref="currentPoints"/>
        /// </summary>
        /// <param name="_PointsToSubtract">The points to subtract from <see cref="currentPoints"/></param>
        private void SubtractPoints(uint _PointsToSubtract) 
        {
            this.SetPoints(-(int)_PointsToSubtract);
        }
        
        /// <summary>
        /// Adds the given points to <see cref="currentPoints"/> <br/>
        /// <i>Set to 0, to reset <see cref="currentPoints"/> and <see cref="pointsDelta"/></i>
        /// </summary>
        /// <param name="_Points">The points to add/subtract to <see cref="currentPoints"/></param>
        private void SetPoints(int _Points)
        {
            if (_Points == 0)
            {
                this.currentPoints = 0;
                this.pointsDelta = 1; // pointsDelta must be different from currentPoints, otherwise the Coroutine won't run
            }
            else
            {
                this.currentPoints = (uint)Mathf.Clamp(this.currentPoints + _Points, 0, uint.MaxValue);
            }

            if (this.pointsCoroutine == null)
            {
                this.pointsCoroutine = SetPoints();
                StartCoroutine(this.pointsCoroutine);
            }
            
            OnPointsChanged?.Invoke(this.currentPoints);
        }

        /// <summary>
        /// Gradually increase/decreases the points over time
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetPoints()
        {
#if UNITY_EDITOR
            // That way the editor doesn't have to be restarted when the value of "pointsWaitTime" is adjusted
            this.pointsWaitForSeconds = new WaitForSeconds(this.pointsWaitTime);
#endif
            
            while (this.pointsDelta != this.currentPoints)
            {
                if (this.pointsDelta < this.currentPoints)
                {
                    this.pointsDelta++;
                }
                else if (this.pointsDelta > this.currentPoints)
                {
                    this.pointsDelta--;
                }
                
                this.SetPointsText(this.pointsDelta);
                
                yield return this.pointsWaitForSeconds;
            }
            
            this.pointsCoroutine = null;
        }
        
        /// <summary>
        /// Sets the <see cref="TextMeshProUGUI.text"/> of <see cref="pointsAmount"/> to the given value + 'P'
        /// </summary>
        /// <param name="_Points"></param>
        private void SetPointsText(uint _Points)
        {
            this.pointsAmount.text = string.Concat(_Points, 'P');
        }

        /// <summary>
        /// <see cref="MenuContainer.OnNewBestScore"/>
        /// </summary>
        /// <param name="_NewBestScore">The new best score amount</param>
        private void NewBestScore(uint _NewBestScore)
        {
            this.bestScoreAmount.text = _NewBestScore.ToString();
        }

        /// <summary>
        /// Sets <see cref="currentPoints"/> to 0
        /// </summary>
        /// <param name="_ResetReason">Not needed here</param>
        private void ResetPoints(ResetReason _ResetReason)
        {
            this.SetPoints(0);
        }
        
#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// <see cref="AddPoints"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to get the points for</param>
        /// <param name="_Multiplier">Multiplier for the added points</param>
        public static void AddPoints_DEVELOPMENT(Fruit _Fruit, float _Multiplier)
        {
            Instance.multiplier.StartMultiplier();

            var _points = (int)(((int)_Fruit + Instance.multiplier.CurrentMultiplier) * _Multiplier);
            Instance.SetPoints(_points);
        }
        
        /// <summary>
        /// <see cref="SubtractPoints"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        /// <param name="_PointsToSubtract">The points to subtract from <see cref="currentPoints"/></param>
        /// <param name="_Multiplier">Multiplier for the subtracted points</param>
        public static void SubtractPoints_DEVELOPMENT(uint _PointsToSubtract, float _Multiplier)
        {
            Instance.SetPoints(-(int)(_PointsToSubtract * _Multiplier));
        }
#endif
        #endregion
    }
}