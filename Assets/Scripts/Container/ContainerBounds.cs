using System;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Container
{
    /// <summary>
    /// Handles all logic for one container
    /// </summary>
    internal sealed class ContainerBounds : GameModeTransition
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Inside bounds of the container")]
        [SerializeField] private RectTransform bounds;
        [Tooltip("Trigger of MaxHeight")]
        [SerializeField] private BoxCollider2D maxHeightTrigger;

        [Header("Settings")]
        [Tooltip("Y-position of the FruitSpawner")]
        [ShowInInspector] private static ProtectedFloat fruitSpawnerHeight = 16;
        
        [Header("Debug")]
        [Tooltip("The connection Id of the player that is assigned to this container")]
        [ShowInInspector][ReadOnly] private ProtectedInt32? connectionId;
        #endregion

        #region Fields
        /// <summary>
        /// Reference to the <see cref="FruitSpawner"/> that is assigned to this container <br/>
        /// <b>Will only be set for the local client, for every other client this will be null!</b>
        /// </summary>
        [CanBeNull] private FruitSpawner fruitSpawner;
        #endregion
        
        #region Properties
        /// <summary>
        /// True if this container is assigned to the local client <br/>
        /// False if it assigned to another client
        /// </summary>
        public ProtectedBool PlayerContainer { get; private set; }
        /// <summary>
        /// <see cref="connectionId"/>
        /// </summary>
        public ProtectedInt32? ConnectionId => this.connectionId;
        /// <summary>
        /// Starting position of the <see cref="FruitSpawner"/>
        /// </summary>
        public ProtectedVector2 StartingPosition { get; private set; } = new Vector2(0, fruitSpawnerHeight);
        #endregion
        
        #region Methods
        private void Start()
        {
            if (!GameController.Containers.Contains(this))
            {
                Destroy(base.gameObject);
            }
            
            this.bounds.GetComponent<Canvas>().worldCamera = CameraUtils.Camera;
        }

        protected override void Transition(GameMode _GameMode)
        {
            if (this.fruitSpawner != null)
            {
                this.fruitSpawner.GameModeTransitionStarted();
            }
            
            base.Transition(_GameMode);
            
            AudioPool.PlayClip(AudioClipName.FruitDestroy);
        }

        /// <summary>
        /// Assigns this <see cref="ContainerBounds"/> to the given <see cref="FruitSpawner"/> <br/>
        /// <i>Use for client container</i>
        /// </summary>
        /// <param name="_FruitSpawner">The <see cref="FruitSpawner"/> to assign this <see cref="ContainerBounds"/> to</param>
        public void AssignToPlayer(FruitSpawner _FruitSpawner)
        {
            this.fruitSpawner = _FruitSpawner;
            this.connectionId = this.fruitSpawner!.SetContainerBounds(this);
            this.PlayerContainer = true;
            this.maxHeightTrigger.enabled = true;
        }

        /// <summary>
        /// Assigns this container a connection id <br/>
        /// <i>Use for container of other clients</i>
        /// </summary>
        /// <param name="_ConnectionId">The connection id to assign to this container</param>
        public void AssignConnectionId(int _ConnectionId)
        {
            this.connectionId = _ConnectionId;
            this.PlayerContainer = false;
            this.maxHeightTrigger.enabled = false;
        }
        
        /// <summary>
        /// Sets <see cref="fruitSpawner"/> and <see cref="connectionId"/> to null
        /// </summary>
        public void FreeContainer()
        {
            this.fruitSpawner = null;
            this.connectionId = null;
        }

        /// <summary>
        /// Sets the <see cref="StartingPosition"/> for the <see cref="FruitSpawner"/> <br/>
        /// <i>
        /// Call this after the container has been positioned correctly <br/>
        /// Currently called at the end of the transition animation
        /// </i>
        /// </summary>
        public void SetStartingPosition()
        {
            this.StartingPosition = new Vector2(base.transform.position.x, fruitSpawnerHeight);
            
            if (this.fruitSpawner != null)
            {
                this.fruitSpawner.GameModeTransitionEnded();
            }
        }
        
        /// <summary>
        /// Returns true if the x and y components of point is a point inside <see cref="bounds"/>
        /// </summary>
        /// <param name="_Point">Point to test</param>
        /// <returns>True if the point lies within the specified rectangle</returns>
        public bool Contains(Vector2 _Point)
        {
            return this.bounds.rect.Contains(new Vector3(_Point.x, _Point.y) - this.bounds.position);
        }
        #endregion
    }
}