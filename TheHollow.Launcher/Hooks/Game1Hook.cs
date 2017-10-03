using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Kerberos.Steam;
using Microsoft.Xna.Framework.Audio;
using ThePit;
using ThePit.GameUI;
using ThePit.UI;

namespace TheHollow.Launcher
{
    /// <summary>
    /// This helps us launch the game and inject some of our custom stuff.
    /// </summary>
    public class Game1Hook : Game1
    {
        public Game1Hook() : base()
        {
        }

        public override void Initialize()
        {
            // inject our own content handler
            this.Content = new ContentManagerHook(this.Content, this.GraphicsDevice);

            // override the static version of the content manager in case we need it
            Game1.SpriteManager.GeneralContent = this.Content;

            base.Initialize();
        }

        public override void LoadContent()
        {
            // first run the default Pit content load process
            base.LoadContent();
            //base.LoadContentNow();

            //// now go back and inject our own ingame screen handler
            //// (this allows us to hook things like player input and in game menus)
            //var screen = (Screen)new InGameScreenHook();
            //screen.Name = Names.Screens.InGame;
            //this.uiManager._screens[Names.Screens.InGame] = screen;

            //// inject select player and main menu screens
            //// (so we can add characters)
            //screen = (Screen)new SelectPlayerScreenHook();
            //screen.Name = Names.Screens.SelectPlayer;
            //this.uiManager._screens[Names.Screens.SelectPlayer] = screen;

            //screen = (Screen)new MainMenuScreenHook();
            //screen.Name = Names.Screens.MainMenu;
            //this.uiManager._screens[Names.Screens.MainMenu] = screen;
        }

        public override void LoadContentNow()
        {


            this._audioEngine = new AudioEngine(Path.Combine(this.Content.RootDirectory, "Content/audio/the_pit.xgs"));
            this._effectsWaveBank = new WaveBank(this._audioEngine, Path.Combine(this.Content.RootDirectory, "Content/audio/fx.xwb"));
            this._speechWaveBank = new WaveBank(this._audioEngine, Path.Combine(this.Content.RootDirectory, "Content/audio/speech.xwb"), 0, (short)16);
            this._effectsSoundBank = new SoundBank(this._audioEngine, Path.Combine(this.Content.RootDirectory, "Content/audio/fx.xsb"));
            this._musicWaveBank = new WaveBank(this._audioEngine, Path.Combine(this.Content.RootDirectory, "Content/audio/music.xwb"), 0, (short)16);
            this._musicSoundBank = new SoundBank(this._audioEngine, Path.Combine(this.Content.RootDirectory, "Content/audio/music.xsb"));
            this._tutorialWaveBank = new WaveBank(this._audioEngine, Path.Combine(this.Content.RootDirectory, "Content/audio/tutorial.xwb"), 0, (short)16);
            this._tutorialSoundBank = new SoundBank(this._audioEngine, Path.Combine(this.Content.RootDirectory, "Content/audio/tutorial.xsb"));


            this._audioEngine = new AudioEngine("Content/audio/the_pit.xgs");
            this._effectsWaveBank = new WaveBank(this._audioEngine, "Content/audio/fx.xwb");
            this._speechWaveBank = new WaveBank(this._audioEngine, "Content/audio/speech.xwb", 0, (short)16);
            this._effectsSoundBank = new SoundBank(this._audioEngine, "Content/audio/fx.xsb");
            this._musicWaveBank = new WaveBank(this._audioEngine, "Content/audio/music.xwb", 0, (short)16);
            this._musicSoundBank = new SoundBank(this._audioEngine, "Content/audio/music.xsb");
            this._tutorialWaveBank = new WaveBank(this._audioEngine, "Content/audio/tutorial.xwb", 0, (short)16);
            this._tutorialSoundBank = new SoundBank(this._audioEngine, "Content/audio/tutorial.xsb");

            Game1.RunContext.Sound.Engine = this._audioEngine;
            Game1.RunContext.Sound.Effects = this._effectsSoundBank;
            Game1.RunContext.Sound.Music = this._musicSoundBank;
            Game1.RunContext.Sound.Tutorial = this._tutorialSoundBank;
            Data.Initialize(this.Content);
            Game1.SpriteManager.Initialize();
            if (API.IsAvailable)
            {
                if (Profiles.Current.ForceCloudDisabled)
                {
                    Profiles.Current.ForceCloudDisabled = false;
                    Log.Trace("Disabling cloud storage by default for pre-existing profile.");
                    API.SetCloudEnabledForApp(false);
                }
                Game1.RunContext.StatsAndAchievements = new ThePit.StatsAndAchievements();
            }
            this.graphicsContext.DefaultFont = Data.GetFont(Names.Assets.DefaultFont);
            this.graphicsContext.DebugFont = Data.GetFont(Names.Assets.StatsFont);
            this.graphicsContext.WorldRenderer = new OrthographicRenderer();
            this.graphicsContext.WorldRenderer.Init(this.Content);
            this.uiManager = new Manager((IGameMessageQueue)this, this.Content, this.graphicsContext.DefaultFont);
            this.uiManager.MainPanel.Size = Tweaks.NativeScreenSize.ToVector2();
            this.uiManager.AddScreen(Names.Screens.MainMenu, (Screen)new MainMenuScreen());
            this.uiManager.AddScreen(Names.Screens.InGame, (Screen)new InGameScreen());
            this.uiManager.AddScreen(Names.Screens.SelectPlayer, (Screen)new SelectPlayerScreen());
            this.uiManager.AddScreen(Names.Screens.GameOver, (Screen)new GameOverScreen());
            this.uiManager.AddScreen(Names.Screens.Logo, (Screen)new LogoScreen());
            this.uiManager.AddScreen(Names.Screens.Cinematic, (Screen)new CinematicScreen());
            this.uiManager.AddScreen(Names.Screens.Scores, (Screen)new ScoresScreen());
            this.uiManager.AddScreen(Names.Screens.GameOptions, (Screen)new GameOptionsScreen());
            this.uiManager.AddScreen(Names.Screens.DemoEnd, (Screen)new DemoEndSplash());
            this.uiManager.AddScreen(Names.Screens.DemoExit, (Screen)new DemoExitSplash());
            this.uiManager.AddScreen(Names.Screens.DebugAnimations, (Screen)new DebugAnimations());
            foreach (VolumeSetting setting in Volume.Settings)
                this.SetVolume(setting.Category, Profiles.Current.GetVolume(setting.Category));
            Profiles.Current.LoadKeyBindings(KeyCommands.Commands);
            Profiles.Current.LoadGamePadBindings(GamePadCommands.Commands);
            if (this.GraphicsContext.SupportedDisplayResolutions.Contains<DisplayResolution>(Profiles.Current.DisplayResolution))
                this.DisplayResolution = Profiles.Current.DisplayResolution;
            GameSetupParams startGameParams = this.GetStartGameParams();
            if (startGameParams != null)
                this.NewGame(startGameParams);
            else
                this.uiManager.ShowScreen(Names.Screens.Logo);


            // now go back and inject our own ingame screen handler
            // (this allows us to hook things like player input and in game menus)
            var screen = (Screen)new InGameScreenHook();
            screen.Name = Names.Screens.InGame;
            this.uiManager._screens[Names.Screens.InGame] = screen;

            // inject select player and main menu screens
            // (so we can add characters)
            screen = (Screen)new SelectPlayerScreenHook();
            screen.Name = Names.Screens.SelectPlayer;
            this.uiManager._screens[Names.Screens.SelectPlayer] = screen;

            screen = (Screen)new MainMenuScreenHook();
            screen.Name = Names.Screens.MainMenu;
            this.uiManager._screens[Names.Screens.MainMenu] = screen;
        }

        /// <summary>
        /// This is how we hook keyboard/other input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="allowFull"></param>
        /// <returns></returns>
        public override bool UpdatePlayerActionInput(InputState input, bool allowFull)
        {
            // this is a simple, hard coded example to handle input from a player
            // we can probably add some kind of modifier key here for doing something else cool
            if (input.IsKeyReleased(Keys.OemPlus))
            {
                bool canAutoTarget = false;
                bool canTargetSpace = false;
                bool isAutoTarget;
                bool isTargetBlocked;

                // find something to kill
                ITarget targetHere = this._world.Cursor.GetTargetHere(canAutoTarget, canTargetSpace, out isAutoTarget, out isTargetBlocked);
                if (null != targetHere)
                {
                    // output some data to the debug log
                    DebugLog.Write("Terminating target: {0}", targetHere.DisplayName);

                    // kill it
                    Damage.ApplyDamage(targetHere, targetHere.Health);
                }
            }

            return base.UpdatePlayerActionInput(input, allowFull);
        }
        
        

    }
}
