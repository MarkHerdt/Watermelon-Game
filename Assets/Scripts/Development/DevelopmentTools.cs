using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using TMPro;
using UnityEngine;
using Watermelon_Game.Container;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Fruits;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Points;
using Watermelon_Game.Skills;
using Watermelon_Game.Utility;
using AudioSettings = Watermelon_Game.Audio.AudioSettings;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Development
{
    /// <summary>
    /// Tool for use only in editor or in development builds
    /// </summary>
    public class DevelopmentTools : NetworkBehaviour
    {
#if DEBUG || DEVELOPMENT_BUILD
        [Header("References")]
        [Tooltip("Displays the build number of a Debug-build (For development only)")]
        [SerializeField] private TextMeshProUGUI developmentVersion;
        [Tooltip("Used to display information for certain actions")]
        [SerializeField] private TextMeshProUGUI infoText;
#endif
        
#if UNITY_EDITOR
        #region Inspector Fields
        [Header("Settings")]
        [Tooltip("Displays which key was last pressed in a Debug.Log")]
        [SerializeField] private bool logKey;
        #endregion
#endif
        
#if DEBUG || DEVELOPMENT_BUILD
        #region Constants
        /// <summary>
        /// Name + extension for the DEVELOPMENT_VERSION.txt file
        /// </summary>
        public const string DEVELOPMENT_VERSION = "DEVELOPMENT_VERSION.txt";
        #endregion
        
        #region Fields
#pragma warning disable CS0109
        /// <summary>
        /// Main <see cref="Camera"/> in the scene
        /// </summary>
        private new Camera camera;
#pragma warning restore CS0109
        
        /// <summary>
        /// The currently selected <see cref="FruitBehaviour"/>
        /// </summary>
        [CanBeNull] private FruitBehaviour currentFruit;
        private readonly List<SavedFruit> savedFruits = new();
        
        /// <summary>
        /// The key that was last pressed
        /// </summary>
        private KeyCode? lastPressedKey;
        /// <summary>
        /// Indicates whether the fruits on the map are currently frozen or not
        /// </summary>
        private bool fruitsAreFrozen;
        #endregion
#endif
        
        #region Methods
        private void Awake()
        {
#if DEBUG || !DEVELOPMENT_BUILD
            if (!Application.isEditor && !Debug.isDebugBuild)
            {
                Destroy(this.gameObject);
                return;
            }      
#endif

#if DEBUG || DEVELOPMENT_BUILD
            this.camera = Camera.main;
            
            var _developmentVersionPath = Path.Combine(Application.dataPath, DEVELOPMENT_VERSION);
            this.developmentVersion.text = File.ReadAllText(_developmentVersionPath);
#endif
        }
        
#if DEBUG || DEVELOPMENT_BUILD
        private void Update()
        {
            this.ReplaceWithGrape();
            this.ForceGoldenFruit();
            this.SpawnFruit(KeyCode.F1, Fruit.Grape);
            this.SpawnFruit(KeyCode.F2, Fruit.Cherry);
            this.SpawnFruit(KeyCode.F3, Fruit.Strawberry);
            this.SpawnFruit(KeyCode.F4, Fruit.Lemon);
            this.SpawnFruit(KeyCode.F5, Fruit.Orange);
            this.SpawnFruit(KeyCode.F6, Fruit.Apple);
            this.SpawnFruit(KeyCode.F7, Fruit.Pear);
            this.SpawnFruit(KeyCode.F8, Fruit.Pineapple);
            this.SpawnFruit(KeyCode.F9, Fruit.Honeymelon);
            this.SpawnFruit(KeyCode.F10, Fruit.Watermelon);
            this.FollowMouse();
            this.ReleaseFruit();
            this.SetReleaseBlock();
            this.DisableCountdown();
            this.SetCurrentFruit();
            this.DeleteFruit();
            this.FreezeFruitPositions();
            this.SaveFruitsOnMap();
            this.LoadFruit();
            this.SpawnUpgradedFruit();
            this.SetPointsAndFill();
            this.SetBackgroundMusic();
        }

        /// <summary>
        /// Replaces the <see cref="Fruit"/> on the <see cref="FruitSpawner"/> with a <see cref="Fruit.Grape"/>
        /// </summary>
        private void ReplaceWithGrape()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                FruitSpawner.ForceFruit_DEVELOPMENT(Fruit.Grape);
            }
        }

        /// <summary>
        /// Forces the <see cref="FruitSpawner.fruitBehaviour"/> that is currently held by the <see cref="FruitSpawner"/> to become a golden fruit
        /// </summary>
        private void ForceGoldenFruit()
        {
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                FruitSpawner.ForceGoldenFruit_DEVELOPMENT();
            }
        }
        
        /// <summary>
        /// Spawns the given <see cref="Fruit"/> at the position of the mouse (in world space)
        /// </summary>
        /// <param name="_KeyCode">Key that needs to be pressed to spawn the <see cref="Fruit"/></param>
        /// <param name="_Fruit">The <see cref="Fruit"/> to spawn</param>
        private void SpawnFruit(KeyCode _KeyCode, Fruit _Fruit)
        {
            if (Input.GetKeyDown(_KeyCode))
            {
                if (this.currentFruit != null)
                {
                    this.currentFruit!.DestroyFruit();
                    this.currentFruit = null;
                    
                    if (this.lastPressedKey == _KeyCode)
                    {
                        return;   
                    }
                }
                
                this.lastPressedKey = _KeyCode;
                var _mouseWorldPosition = this.camera.ScreenToWorldPoint(Input.mousePosition);
                this.currentFruit = this.SpawnFruit(_mouseWorldPosition.WithZ(0), _Fruit, Quaternion.identity);
            }
        }

        /// <summary>
        /// Makes the <see cref="currentFruit"/> follow the position of the mouse (in world space)
        /// </summary>
        private void FollowMouse()
        {
            if (this.currentFruit != null)
            {
                var _mouseWorldPosition = this.camera.ScreenToWorldPoint(Input.mousePosition).WithZ(0);
                if (this.currentFruit.Rigidbody2D_DEVELOPMENT.simulated)
                {
                    this.currentFruit.Rigidbody2D_DEVELOPMENT.MovePosition(_mouseWorldPosition);   
                }
                else
                {
                    this.currentFruit.transform.position = _mouseWorldPosition;
                }
            }
        }

        /// <summary>
        /// Releases the <see cref="currentFruit"/> from the mouse and sets <see cref="currentFruit"/> to null
        /// </summary>
        private void ReleaseFruit()
        {
            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                if (this.currentFruit != null)
                {
                    this.currentFruit.CmdRelease(new Vector2(0, -1), false);
                    this.currentFruit = null;
                }
            }
        }

        /// <summary>
        /// Enables/disables <see cref="FruitSpawner.NoReleaseBlock"/>
        /// </summary>
        private void SetReleaseBlock()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                FruitSpawner.NoReleaseBlock = !FruitSpawner.NoReleaseBlock;

                ShowInfoText(FruitSpawner.NoReleaseBlock ? "Release Unblocked" : "Release Blocked");
            }
        }

        /// <summary>
        /// Disables/enables <see cref="MaxHeight.DisableCountDown"/>
        /// </summary>
        private void DisableCountdown()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                MaxHeight.DisableCountDown = !MaxHeight.DisableCountDown;
                
                ShowInfoText(MaxHeight.DisableCountDown ? "Countdown Disabled" : "Countdown Enabled");
            }
        }
        
        /// <summary>
        /// Makes the clicked on <see cref="Fruit"/> the <see cref="currentFruit"/> so that it follows the mouse, while the mouse button is held down <br/>
        /// <see cref="Fruit"/> will be dropped on mouse release
        /// </summary>
        private void SetCurrentFruit()
        {
            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                if (this.currentFruit == null)
                {
                    var _raycastHit2D = this.FruitRaycast();

                    if (_raycastHit2D)
                    {
                        var _fruitBehaviour = _raycastHit2D.transform.gameObject.GetComponent<FruitBehaviour>();
                        
                        this.currentFruit = _fruitBehaviour;
                    }   
                }
            }
            if (Input.GetKeyUp(KeyCode.Mouse2))
            {
                if (this.currentFruit != null)
                {
                    
                    this.currentFruit!.Rigidbody2D_DEVELOPMENT.velocity = Vector2.zero;
                    this.currentFruit!.Rigidbody2D_DEVELOPMENT.angularVelocity = 0;
                }
                
                this.currentFruit = null;
            }  
        }
        
        /// <summary>
        /// Destroys the <see cref="Fruit"/> under the mouse
        /// </summary>
        private void DeleteFruit()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (this.currentFruit == null)
                {
                    var _raycastHit2D = this.FruitRaycast();
                    
                    if (_raycastHit2D)
                    {
                        _raycastHit2D.transform.gameObject.GetComponent<FruitBehaviour>().DestroyFruit();
                    }
                }
            }
        }

        /// <summary>
        /// Sends a raycast from the mouse position into world space that checks if it hit a <see cref="Fruit"/>
        /// </summary>
        /// <returns></returns>
        private RaycastHit2D FruitRaycast()
        {
            var _ray = this.camera.ScreenPointToRay(Input.mousePosition);
            var _raycastHit2D = Physics2D.Raycast(_ray.origin, _ray.direction, Mathf.Infinity, LayerMaskController.FruitMask);
            Debug.DrawRay(_ray.origin, _ray.direction * 100, Color.red, 5);

            return _raycastHit2D;
        }

        /// <summary>
        /// Freezes/unfreezes the position of all fruits on the map
        /// </summary>
        private void FreezeFruitPositions()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                this.fruitsAreFrozen = !this.fruitsAreFrozen;
                this.ShowInfoText(this.fruitsAreFrozen ? "FROZEN" : "UNFROZEN");

                foreach (var _fruitBehaviour in FruitController.Fruits)
                {
                    _fruitBehaviour.Freeze_DEVELOPMENT(this.fruitsAreFrozen);
                }
            }
        }
        
        /// <summary>
        /// Saves the position and rotation of all <see cref="Fruit"/>s on the map
        /// </summary>
        private void SaveFruitsOnMap()
        {
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                this.savedFruits.Clear();
                
                foreach (var _fruitBehaviour in FruitController.Fruits)
                {
                    var _savedFruit = new SavedFruit(_fruitBehaviour);
                    this.savedFruits.Add(_savedFruit);
                }
                
                this.ShowInfoText("SAVED");
            }
        }
        
        /// <summary>
        /// Instantiates the <see cref="Fruit"/>s that have been saved to <see cref="savedFruits"/>
        /// </summary>
        private void LoadFruit()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (this.savedFruits.Count > 0)
                {
                    var _fruitsOnMap = FruitController.Fruits;
                
                    // ReSharper disable once InconsistentNaming
                    for (var i = _fruitsOnMap.Count - 1; i >= 0; i--)
                    {
                        _fruitsOnMap.ElementAt(i).DestroyFruit();
                    }

                    foreach (var _savedFruit in this.savedFruits)
                    {
                        this.SpawnFruit(_savedFruit.Position, _savedFruit.Fruit, _savedFruit.Rotation).CmdRelease(new Vector2(0, -1), false);
                    }
#if UNITY_EDITOR
                    Debug.Log($"{this.savedFruits.Count} Fruits spawned.");
#endif
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("No fruits are currently saved.");
#endif
                }
            }
        }
        
        /// <summary>
        /// Spawns an upgraded golden <see cref="Fruit"/>
        /// </summary>
        private void SpawnUpgradedFruit()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                var _fruitBehaviour = this.SpawnFruit(base.transform.position.WithY(CameraUtils.YFrustumPosition + 15), Fruit.Grape, Quaternion.identity);
                _fruitBehaviour.ForceGoldenFruit_DEVELOPMENT();
                _fruitBehaviour.CmdRelease(new Vector2(0, -1), false);
            }
        }
        
        /// <summary>
        /// Spawns a <see cref="Fruit"/>
        /// </summary>
        /// <param name="_Position">Position to spawn the <see cref="Fruit"/> at</param>
        /// <param name="_Fruit">The <see cref="Fruit"/> to spawn</param>
        /// <param name="_Rotation">The rotation to spawn the <see cref="Fruit"/> with</param>
        /// <returns></returns>
        private FruitBehaviour SpawnFruit(Vector3 _Position, Fruit _Fruit, Quaternion _Rotation)
        {
            var _fruitBehaviour = FruitBehaviour.SpawnFruit(FruitContainer.Transform, _Position, _Rotation, (int)_Fruit, true);

            return _fruitBehaviour;
        }
        
        /// <summary>
        /// Adds or subtract a random amount of points
        /// </summary>
        private void SetPointsAndFill()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                var _maxFruit = Enum.GetValues(typeof(Fruit)).Length - 1;
                var _randomFruit = (Fruit)Random.Range(0, _maxFruit);
                
                PointsController.AddPoints_DEVELOPMENT(_randomFruit, 10);
                StoneFruitCharge.SetFill_DEVELOPMENT(1);
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                var _randomNumber = (uint)Random.Range(1, 10);
                
                PointsController.SubtractPoints_DEVELOPMENT(_randomNumber, 1);
                StoneFruitCharge.SetFill_DEVELOPMENT(-1);
            }
        }
        
        /// <summary>
        /// Enables/disables the background music
        /// </summary>
        private void SetBackgroundMusic()
        {
            if (Input.GetKeyDown(KeyCode.Slash)) // Minus on german layout
            {
                AudioSettings.FlipBGM_DEVELOPMENT();
            }
        }
        
        /// <summary>
        /// Sets enables the <see cref="TextMeshProUGUI.text"/> of <see cref="infoText"/> to the given <br/>
        /// <i>The <see cref="TextMeshProUGUI.text"/> will be disabled after 1 seconds</i>
        /// </summary>
        /// <param name="_Text">The value to set the <see cref="TextMeshProUGUI.text"/> to</param>
        private void ShowInfoText(string _Text)
        {
            this.infoText.text = _Text;
            this.infoText.enabled = true;
            Invoke(nameof(HideInfoText), 1);
        }
        
        /// <summary>
        /// Disables <see cref="infoText"/>
        /// </summary>
        private void HideInfoText()
        {
            this.infoText.enabled = false;
        }
#endif

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (this.logKey)
            {
                var _currentEvent = Event.current;
                if (_currentEvent.isKey)
                {
                    Debug.Log(_currentEvent.keyCode);
                }
            }
        }
#endif
        #endregion
    }
}
