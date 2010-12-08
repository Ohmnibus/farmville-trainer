using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MouseKeyboardLibrary;

namespace Ohm.FarmVille {
	
	/// <summary>
	/// Tool per guadagnare un po' di punti a FarmVille.
	/// </summary>
	public partial class FarmVilleTrainer : Form {
		
		public abstract class BaseFarmer {

			#region Campi privati/protetti

			protected int Status = 99;
			protected int SubStatus = 0;

			protected Point StartingPoint = new Point();

			protected Timer MyTimer;
			private MouseHook MyMouseHook;
			private KeyboardHook MyKeyboardHook;

			#endregion

			#region Intervalli temporali

			/// <summary>
			/// Attesa prima di iniziare le operazioni.
			/// </summary>
			protected const int DELAY_STARTUP = 5000;
			/// <summary>
			/// Tempo necessario per il completamento di un operazione standard.
			/// </summary>
			protected const int DELAY_STD_OP = 2500;
			/// <summary>
			/// Tempo necessario per la raccolta/aratura/semina.
			/// </summary>
			protected const int DELAY_PREV_OP = 1100;
			/// <summary>
			/// Tempo necessario per la selezione di una casella.
			/// </summary>
			protected const int DELAY_FAST = 250; //250
			/// <summary>
			/// Salto all'operazione successiva.
			/// </summary>
			protected const int DELAY_NULL = 75;

			#endregion

			#region Locazioni

			/// <summary>
			/// Risoluzione di riferimento.
			/// </summary>
			static Size refRes = new Size(1280, 1024);

			/// <summary>
			/// Risoluzione attuale.
			/// </summary>
			Size actRes; //new Size(1280, 1024);

			Size topMod;
			Size midMod;
			Size botMod;

			protected const int TOOL_CURSOR = 0;
			protected const int TOOL_PLOW = 1;
			protected const int TOOL_COOP = 2;
			protected const int TOOL_RIBBON = 3;
			protected const int TOOL_MARKET = 4;
			protected const int TOOL_GIFT = 5;

			protected const int SUBTOOL_CURSOR_CURSOR = 0;
			protected const int SUBTOOL_CURSOR_MOVE = 1;
			protected const int SUBTOOL_CURSOR_RECYCLE = 2;

			protected const int SUBTOOL_PLOW_PLOW = 0;
			protected const int SUBTOOL_PLOW_MULTITOOL = 1;
			protected const int SUBTOOL_PLOW_HARVESTER = 2;
			protected const int SUBTOOL_PLOW_SEEDER = 3;
			protected const int SUBTOOL_PLOW_PLANE = 4;
			protected const int SUBTOOL_PLOW_TRACTOR = 5;

			protected static Point[] _toolPlaces = new Point[] { 
				//Multi Tool, Plow Tool, Co-Op
				new Point(900, 930), new Point(948, 930), new Point(996, 930),
				//Ribbons, Market, Gifts
				new Point(900, 988), new Point(948, 988), new Point(996, 988)
			};

			protected static Point[][] _toolSubPlaces = new Point[][] { 
				new Point[] { //Multi Tool
					new Point(900, 865), //Multi Tool
					new Point(900, 800), //Move Tool
					new Point(900, 735)  //Recycle
				},
				new Point[] { //Plow Tool
					new Point(948, 865), //Plow
					new Point(948, 800), //Multitool
					new Point(948, 735), //Harvester
					new Point(948, 670), //Seeder
					new Point(948, 600), //Plane
					new Point(948, 535)  //Tractor
				},
				new Point[] {},
				new Point[] {},
				new Point[] {},
				new Point[] {}
			};

			protected const int MARKET_TYPE_SPECIALS = 0;
			protected const int MARKET_TYPE_SEEDS_TREES = 1;
			protected const int MARKET_TYPE_ANIMALS = 2;
			protected const int MARKET_TYPE_BUILDINGS = 3;
			protected const int MARKET_TYPE_DECORATIONS = 4;
			protected const int MARKET_TYPE_FARM_AIDES = 5;
			protected const int MARKET_TYPE_CLOTES = 6;

			protected static Point[] _marketTypePlaces = new Point[] { 
				new Point(350, 320), //Specials
				new Point(450, 320), //Seeds & Trees
				new Point(550, 320), //Animals
				new Point(640, 320), //Buildings
				new Point(740, 320), //Decorations
				new Point(840, 320), //Farm Aides
				new Point(930, 320)  //Clotes
			};

			//12/05/2010: Cambio dimensioni da 25, 12 a 20, 9.6
			//04/07/2010: Cambio dimensioni da 20, 9.6 a 15, 7.2
			protected static Size OffsetNextRight = new Size(15, 7);
			protected static Size SubOffsetNextRight = new Size(0, 250); //Millesimi da aggiungere a OffsetNextRight
			protected static Size OffsetNextLeft = new Size(-15, 7);
			protected static Size SubOffsetNextLeft = new Size(0, 250); //Millesimi da aggiungere a OffsetNextLeft

			protected static Size OffsetNextBaleRight = new Size(25, 12);
			protected static Size OffsetNextBaleLeft = new Size(-25, 12);

			protected static Size OffsetSell = new Size(32, 32);
			protected static Size OffsetSelectBale = new Size(0, -22);

			/// <summary>
			/// 1x1 size element to adjust zero based points
			/// </summary>
			protected static Size adj = new Size(1, 1);

			public const int SEED_PER_PAGE = 6;
			public const int SEED_COLUMN = 3;

			protected static Point _marketNextPagePlace = new Point(990, 500);

			protected static Point _marketSeedSubPagePlace = new Point(445, 358);

			//137, 196
			protected static Point[] _marketItemPlaces = new Point[] { 
				new Point(497, 482), new Point(723, 565), new Point(950, 565),
				new Point(497, 607), new Point(723, 607), new Point(950, 607)
			};

			//protected static Point _confirmSellPlace = new Point(565, 605);
			//protected static Point _confirmDeletePlace = new Point(565, 605);

			protected static Point _checkDontWarnPlace = new Point(500, 545);
			protected static Point _confirmRecyclePlace = new Point(565, 615);

			#endregion

			#region Proprietà pubbliche

			public bool IsWorking {
				get {
					return Status != 99;
				}
			}

			#endregion

			#region Proprietà private/protette

			//protected Point ConfirmSellPlace {
			//    get {
			//        return _confirmSellPlace + midMod;
			//    }
			//}

			//protected Point ConfirmDeletePlace {
			//    get {
			//        return _confirmDeletePlace + midMod;
			//    }
			//}

			protected Point CheckBoxDontWarnOnRecycle {
				get {
					return _checkDontWarnPlace + midMod;
				}
			}

			protected Point ConfirmRecyclePlace {
				get {
					return _confirmRecyclePlace + midMod;
				}
			}

			protected Point ToolPlace(int tool) {
				return _toolPlaces[tool] + botMod;
			}

			protected Point ToolSubPlace(int tool, int subTool) {
				return _toolSubPlaces[tool][subTool] + botMod;
			}

			protected Point MarketTypePlace(int marketType) {
				return _marketTypePlaces[marketType] + midMod;
			}

			protected Point MarketItemPlace(int marketItem) {
				return _marketItemPlaces[marketItem] + midMod;
			}

			protected Point MarketNextPagePlace {
				get {
					return _marketNextPagePlace + midMod;
				}
			}

			protected Point MarketSeedSubPagePlace {
				get {
					return _marketSeedSubPagePlace + midMod;
				}
			}
			#endregion

			#region ctor, dtor

			public BaseFarmer() {

				actRes = Screen.PrimaryScreen.Bounds.Size;

				topMod = new Size((actRes.Width - refRes.Width) / 2, 0);
				midMod = new Size((actRes.Width - refRes.Width) / 2, (actRes.Height - refRes.Height) / 2);
				botMod = new Size((actRes.Width - refRes.Width) / 2, (actRes.Height - refRes.Height));

				MyTimer = new Timer();
				MyMouseHook = new MouseHook();
				MyKeyboardHook = new KeyboardHook();

				MyTimer.Tick += new EventHandler(MyTimer_Tick);
				MyMouseHook.MouseMove += new MouseEventHandler(MyMouseHook_MouseMove);
				MyKeyboardHook.KeyPress += new KeyPressEventHandler(MyKeyboardHook_KeyPress);
			}

			~BaseFarmer() {
				if (MyMouseHook.IsStarted)
					MyMouseHook.Stop();
				if (MyKeyboardHook.IsStarted)
					MyKeyboardHook.Stop();
			}

			#endregion

			#region Abstract and virtual methods

			abstract protected void MyTimer_Tick(object sender, EventArgs e);

			virtual protected void MyMouseHook_MouseMove(object sender, MouseEventArgs e) {
				MyTimer.Stop();

				if (Status == 0) {
					//Se il mouse si muove rimando l'inizio dell'operazione.

					StartingPoint.X = e.X;
					StartingPoint.Y = e.Y;

					MyTimer.Interval = DELAY_STARTUP;

					MyTimer.Start();
				} else {
					//Stava lavorando. Interrompo.
					Stop();
				}
			}

			virtual protected void MyKeyboardHook_KeyPress(object sender, KeyPressEventArgs e) {
				if (e.KeyChar == 's' || e.KeyChar == 'S') {
					if (Status == 0) {
						//Avvio rapido
						MyTimer.Stop();

						MyTimer.Interval = DELAY_NULL;

						MyTimer.Start();
					} else {
						//Stava lavorando. Interrompo.
						Stop();
					}
				}
			}

			virtual public void Stop() {
				MyTimer.Stop();
				if (MyMouseHook.IsStarted)
					MyMouseHook.Stop();
				if (MyKeyboardHook.IsStarted)
					MyKeyboardHook.Stop();

				Status = 99;
				SubStatus = 0;
			}

			virtual public void Start() {
				Status = 0;
				SubStatus = 0;

				MyMouseHook.Start();
				//MyKeyboardHook.Start();

				MyTimer.Interval = DELAY_STARTUP;
				MyTimer.Start();
			}

			#endregion

			#region Private and protected method

			#region Rimuovi popup

			#region Colors

			/// <summary>
			/// PopUp border color.
			/// </summary>
			protected static Color PopUpBorderColor = Color.FromArgb(112, 84, 58);

			/// <summary>
			/// Green Button color.
			/// </summary>
			protected static Color GreenButtonColor = Color.FromArgb(148, 188, 65);

			/// <summary>
			/// Red Button color.
			/// </summary>
			protected static Color RedButtonColor = Color.FromArgb(234, 21, 21);

			/// <summary>
			/// Dim Red Button color.
			/// </summary>
			protected static Color DimRedButtonColor = Color.FromArgb(195, 70, 58);

			/// <summary>
			/// Fuel Dialog red button.
			/// </summary>
			protected static Color FuelRedButtonColor = DimRedButtonColor;

			/// <summary>
			/// Fuel Dialog green button.
			/// </summary>
			protected static Color FuelGreenButtonColor = GreenButtonColor;

			/// <summary>
			/// Share Dialog red button.
			/// </summary>
			protected static Color ShareRedButtonColor = RedButtonColor;

			/// <summary>
			/// Share Dialog green button.
			/// </summary>
			protected static Color ShareGreenButtonColor = GreenButtonColor;

			/// <summary>
			/// Out Of Sync Dialog green button.
			/// </summary>
			protected static Color OutOfSyncGreenButtonColor = GreenButtonColor;

			/// <summary>
			/// Share Bushel Dialog green button.
			/// </summary>
			protected static Color ShareBushelGreenButtonColor = GreenButtonColor;

			/// <summary>
			/// Share Bushel Dialog red button.
			/// </summary>
			protected static Color ShareBushelRedButtonColor = DimRedButtonColor;

			/// <summary>
			/// Report Bushel Dialog green button.
			/// </summary>
			protected static Color ReportBushelButtonColor = GreenButtonColor;

			/// <summary>
			/// Celebrate Dialog green button.
			/// </summary>
			protected static Color CelebrateGreenButtonColor = GreenButtonColor;

			/// <summary>
			/// Celebrate Dialog red button.
			/// </summary>
			protected static Color CelebrateRedButtonColor = DimRedButtonColor;

			/// <summary>
			/// LevelUp Dialog green button.
			/// </summary>
			protected static Color LevelUpGreenButtonColor = GreenButtonColor;

			/// <summary>
			/// LevelUp Dialog red button.
			/// </summary>
			protected static Color LevelUpRedButtonColor = DimRedButtonColor;

			#endregion

			#region Probe Points

			/// <summary>
			/// List of points to probe for a base PopUp.
			/// </summary>
			protected static Point[] BasePopupBorderProbePoints = new Point[] { 
					/* new Point(455, 376), */ new Point(823, 376), //Removed points that can be overlapped by icon
					/* new Point(441, 390), */ new Point(837, 390),
					new Point(441, 630), new Point(837, 630),
					new Point(455, 643), new Point(823, 643)
				};

			/// <summary>
			/// List of points to probe for a large PopUp.
			/// </summary>
			protected static Point[] LargePopupBorderProbePoints = new Point[] { 
					new Point(322, 295), new Point(962, 295),
					new Point(299, 318), new Point(985, 318),
					new Point(299, 709), new Point(985, 709),
					new Point(322, 732), new Point(962, 732)
				};

			/// <summary>
			/// List of points to probe for a LevelUp PopUp.
			/// </summary>
			protected static Point[] LevelUpPopupBorderProbePoints = new Point[] {
					new Point(483, 314), new Point(798, 314),
					new Point(448, 349), new Point(833, 349),
					new Point(448, 675), new Point(833, 675),
					new Point(483, 710), new Point(798, 710)
				};

			/// <summary>
			/// List of points to probe standard left button.
			/// </summary>
			protected static Point[] LeftButtonProbePoints = new Point[] { 
					new Point(508, 593), new Point(620, 593),
					new Point(508, 617), new Point(620, 617)
				};

			/// <summary>
			/// List of points to probe Fuel Dialog left button.
			/// </summary>
			protected static Point[] FuelLeftButtonProbePoints = new Point[] { 
					new Point(517, 610), new Point(634, 610),
					new Point(517, 634), new Point(634, 634)
				};

			/// <summary>
			/// List of points to probe Fuel Dialog right button.
			/// </summary>
			protected static Point[] FuelRightButtonProbePoints = new Point[] { 
					new Point(656, 610), new Point(767, 610),
					new Point(656, 634), new Point(767, 634)
				};

			/// <summary>
			/// List of points to probe item share Dialog left button.
			/// </summary>
			protected static Point[] ShareLeftButtonProbePoints = new Point[] { 
					new Point(535, 594), new Point(647, 594),
					new Point(535, 618), new Point(647, 618)
				};

			/// <summary>
			/// List of points to probe item share Dialog right button.
			/// </summary>
			protected static Point[] ShareRightButtonProbePoints = new Point[] { 
					new Point(673, 593), new Point(786, 593),
					new Point(673, 618), new Point(786, 618)
				};

			/// <summary>
			/// List of points to probe item celebrate Dialog left button.
			/// </summary>
			protected static Point[] CelebrateLeftButtonProbePoints = new Point[] { 
					new Point(508, 593), new Point(620, 593),
					new Point(508, 617), new Point(620, 617)
				};

			/// <summary>
			/// List of points to probe item celebrate Dialog right button.
			/// </summary>
			protected static Point[] CelebrateRightButtonProbePoints = new Point[] { 
					new Point(648, 593), new Point(759, 593),
					new Point(648, 616), new Point(759, 616)
				};

			/// <summary>
			/// List of points to probe Out of Sync Dialog left button.
			/// </summary>
			protected static Point[] OutOfSyncLeftButtonProbePoints = new Point[] { 
					new Point(508, 593), new Point(620, 593),
					new Point(508, 617), new Point(620, 617)
				};

			/// <summary>
			/// List of points to probe Share Bushel Dialog right button.
			/// </summary>
			protected static Point[] ShareBushelRightButtonProbePoints = new Point[] { 
					new Point(654, 696), new Point(654, 719),
					new Point(765, 696), new Point(765, 719)
				};

			/// <summary>
			/// List of points to probe Share Bushel Dialog left button.
			/// </summary>
			protected static Point[] ShareBushelLeftButtonProbePoints = new Point[] { 
					new Point(517, 695), new Point(517, 718),
					new Point(628, 695), new Point(628, 718)
				};

			/// <summary>
			/// List of points to probe Bushel Report Dialog button.
			/// </summary>
			protected static Point[] ReportBushelButtonProbePoints = new Point[] { 
					new Point(593, 696), new Point(593, 719),
					new Point(704, 696), new Point(704, 719)
				};

			/// <summary>
			/// List of points to probe LevelUp Dialog right button.
			/// </summary>
			protected static Point[] LevelUpRightButtonProbePoints = new Point[] { 
					new Point(653, 671), new Point(764, 671),
					new Point(653, 694), new Point(764, 694)
				};

			/// <summary>
			/// List of points to probe LevelUp Dialog left button.
			/// </summary>
			protected static Point[] LevelUpLeftButtonProbePoints = new Point[] { 
					new Point(519, 671), new Point(631, 671),
					new Point(519, 695), new Point(631, 695)
				};

			#endregion

			/// <summary>
			/// Type of dialogs.
			/// </summary>
			enum DialogType {
				/// <summary>
				/// A dialog has been detected but of unknown type. Should interrupt operations.
				/// </summary>
				Unknown,
				/// <summary>
				/// No popup detected.
				/// </summary>
				None,
				ShareFuel,
				ShareItem,
				Celebrate,
				OutOfSync,
				SavingFarm,
				ShareBushel,
				ReportBushel,
				LevelUp
			}

			/// <summary>
			/// Dismiss a popup.
			/// </summary>
			/// <returns><c>true</c> if no popup or dismissed popup, <c>false</c> for non dismissable popup.</returns>
			protected bool DismissPopup() {
				bool retVal;
				DialogType dialogType;
				bool doLoop;
				bool clicked;

				retVal = true;
				doLoop = true;
				clicked = false;

				while (doLoop && IsWorking) {
					doLoop = false;
					dialogType = CheckPopup();
					switch (dialogType) {
						case DialogType.ShareFuel:
						case DialogType.ShareItem:
						case DialogType.Celebrate:
						case DialogType.ShareBushel:
						case DialogType.ReportBushel:
						case DialogType.LevelUp:
							//Click right button to dismiss.
							if (!clicked) {
								ClickAndWait(GetDismissPoint(dialogType), DELAY_PREV_OP);
								clicked = true;
							} else {
								Wait(DELAY_PREV_OP);
							}
							doLoop = true;
							break;
						case DialogType.OutOfSync:
							//Out Of Sinc. Cannot dismiss.
							retVal = false;
							break;
						case DialogType.SavingFarm:
							//Saving farm. Wait and retry.
							Wait(DELAY_STD_OP);
							doLoop = true;
							break;
						case DialogType.Unknown:
							//Unknown. Stop.
							retVal = false;
							break;
					}
				}

				return retVal;
			}

			/// <summary>
			/// Check if a popup is popped up and it's type.
			/// </summary>
			/// <returns></returns>
			private DialogType CheckPopup() {
				DialogType retVal;

				retVal = DialogType.None;

				if (CheckBasePopup()) {
					if (CheckFuelDialog(false)) {
						retVal = DialogType.ShareFuel;
					} else if (CheckShareDialog(false)) {
						retVal = DialogType.ShareItem;
					} else if (CheckCelebrateDialog(false)) {
						retVal = DialogType.Celebrate;
					} else if (CheckOutOfSyncDialog(false)) {
						retVal = DialogType.OutOfSync;
					} else {
						retVal = DialogType.SavingFarm;
					}
				} else if (CheckLargePopup()) {
					if (CheckShareBushel(false)) {
						retVal = DialogType.ShareBushel;
					} else if (CheckReportBushel(false)) {
						retVal = DialogType.ReportBushel;
					} else {
						//Unknown.
						retVal = DialogType.Unknown;
					}
				} else if (CheckHighPopup()) {
					if (CheckLevelUpDialog(false)) {
						retVal = DialogType.LevelUp;
					} else {
						//Unknown.
						retVal = DialogType.Unknown;
					}
				}

				return retVal;
			}

			/// <summary>
			/// Given a dialog type, return the point to click to dismiss the popup.
			/// </summary>
			/// <param name="dialogType"></param>
			/// <returns></returns>
			private Point GetDismissPoint(DialogType dialogType) {
				Point retVal;

				switch (dialogType) {
					case DialogType.ShareFuel:
						retVal = GetMedianPoint(FuelRightButtonProbePoints) + midMod;
						break;
					case DialogType.ShareItem:
						retVal = GetMedianPoint(ShareRightButtonProbePoints) + midMod;
						break;
					case DialogType.Celebrate:
						retVal = GetMedianPoint(CelebrateRightButtonProbePoints) + midMod;
						break;
					case DialogType.ShareBushel:
						retVal = GetMedianPoint(ShareBushelRightButtonProbePoints) + midMod;
						break;
					case DialogType.ReportBushel:
						retVal = GetMedianPoint(ReportBushelButtonProbePoints) + midMod;
						break;
					case DialogType.LevelUp:
						retVal = GetMedianPoint(LevelUpRightButtonProbePoints) + midMod;
						break;
					default:
						//No dismiss.
						retVal = new Point(0, 0);
						break;
				}

				return retVal;
			}

			#region Small PopUp checking

			/// <summary>
			/// Check if a Base PopUp is popped up.
			/// </summary>
			/// <returns></returns>
			private bool CheckBasePopup() {
				//Bordo
				// 455, 376 - 823, 376
				//441, 390  -  837, 390
				//441, 630  -  837, 630
				// 455, 643 - 823, 643
				//115, 94, 61

				return CheckProbes(BasePopupBorderProbePoints, PopUpBorderColor);
			}

			/// <summary>
			/// Check if a fuel dialog is popped up.
			/// </summary>
			/// <param name="checkBorder">True to ckeck the border, false otherwise.</param>
			/// <returns></returns>
			/// <remarks>
			/// Fuel dialog is the one with 2 tanks saying "While you where plowing you found some extra fuel!" etc.<br />
			/// </remarks>
			private bool CheckFuelDialog(bool checkBorder) {
				bool retVal = true;

				//Bordo
				if (checkBorder) {
					retVal = retVal && CheckProbes(BasePopupBorderProbePoints, PopUpBorderColor);
				}

				//Tasto sinistro ("Share", verde)
				retVal = retVal && CheckProbes(FuelLeftButtonProbePoints, FuelGreenButtonColor);

				//Tasto destro ("Cancel", rosso)
				retVal = retVal && CheckProbes(FuelRightButtonProbePoints, FuelRedButtonColor);

				return retVal;
			}

			/// <summary>
			/// Check if a share dialog is popped up.
			/// </summary>
			/// <returns></returns>
			/// <remarks>
			/// Share dialog is the one shown when an item is found and ask to share the item.
			/// </remarks>
			private bool CheckShareDialog(bool checkBorder) {
				bool retVal = true;

				//Bordo
				if (checkBorder) {
					retVal = retVal && CheckProbes(BasePopupBorderProbePoints, PopUpBorderColor);
				}

				//Tasto sinistro ("Share", verde)
				retVal = retVal && CheckProbes(ShareLeftButtonProbePoints, ShareGreenButtonColor);

				//Tasto destro ("Cancel", rosso)
				retVal = retVal && CheckProbes(ShareRightButtonProbePoints, ShareRedButtonColor);

				return retVal;
			}

			/// <summary>
			/// Check if a celebrate dialog is popped up.
			/// </summary>
			/// <param name="checkBorder"></param>
			/// <returns></returns>
			/// <remarks>
			/// Celebrate dialog is the one shown to celebrate an event like a recipe level-up.
			/// </remarks>
			private bool CheckCelebrateDialog(bool checkBorder) {
				bool retVal = true;

				//Bordo
				if (checkBorder) {
					retVal = retVal && CheckProbes(BasePopupBorderProbePoints, PopUpBorderColor);
				}

				//Tasto sinistro ("Share", verde)
				retVal = retVal && CheckProbes(CelebrateLeftButtonProbePoints, CelebrateGreenButtonColor);

				//Tasto destro ("Cancel", rosso)
				retVal = retVal && CheckProbes(CelebrateRightButtonProbePoints, CelebrateRedButtonColor);

				return retVal;
			}

			/// <summary>
			/// Check if a Out of Sync dialog is popped up.
			/// </summary>
			/// <returns></returns>
			/// <remarks>
			/// Share dialog is the one shown when the game need to be reloaded.
			/// </remarks>
			private bool CheckOutOfSyncDialog(bool checkBorder) {
				bool retVal = true;

				//Bordo
				if (checkBorder) {
					retVal = retVal && CheckProbes(BasePopupBorderProbePoints, PopUpBorderColor);
				}

				//Tasto sinistro ("Accept", verde)
				retVal = retVal && CheckProbes(OutOfSyncLeftButtonProbePoints, OutOfSyncGreenButtonColor);

				return retVal;
			}
			
			#endregion

			#region Large PopUp checking

			/// <summary>
			/// Check if a Large PopUp is popped up.
			/// </summary>
			/// <returns></returns>
			private bool CheckLargePopup() {
				//Bordo

				return CheckProbes(LargePopupBorderProbePoints, PopUpBorderColor);
			}

			/// <summary>
			/// Check the "Bushel Summary" with share option.
			/// </summary>
			/// <param name="checkBorder"></param>
			/// <returns></returns>
			private bool CheckShareBushel(bool checkBorder) {
				bool retVal = true;

				//Bordo
				if (checkBorder) {
					retVal = retVal && CheckProbes(BasePopupBorderProbePoints, PopUpBorderColor);
				}

				//Tasto sinistro ("Share", verde)
				retVal = retVal && CheckProbes(ShareBushelLeftButtonProbePoints, ShareBushelGreenButtonColor);

				//Tasto destro ("Cancel", rosso)
				retVal = retVal && CheckProbes(ShareBushelRightButtonProbePoints, ShareBushelRedButtonColor);

				return retVal;
			}

			/// <summary>
			/// Check the "Bushel Summary" without share option.
			/// </summary>
			/// <param name="checkBorder"></param>
			/// <returns></returns>
			private bool CheckReportBushel(bool checkBorder) {
				bool retVal = true;

				//Bordo
				if (checkBorder) {
					retVal = retVal && CheckProbes(BasePopupBorderProbePoints, PopUpBorderColor);
				}

				//Tasto centrale ("Okay", verde)
				retVal = retVal && CheckProbes(ReportBushelButtonProbePoints, ReportBushelButtonColor);

				return retVal;
			}

			#endregion

			#region LevelUp PopUp checking

			/// <summary>
			/// Check if a High PopUp is popped up.
			/// </summary>
			/// <returns></returns>
			private bool CheckHighPopup() {
				//Bordo

				return CheckProbes(LevelUpPopupBorderProbePoints, PopUpBorderColor);
			}

			/// <summary>
			/// Check if a LevelUp dialog is popped up.
			/// </summary>
			/// <param name="checkBorder"></param>
			/// <returns></returns>
			/// <remarks>
			/// LevelUp dialog is the one shown to celebrate a farmer level-up.
			/// </remarks>
			private bool CheckLevelUpDialog(bool checkBorder) {
				bool retVal = true;

				//Bordo
				if (checkBorder) {
					retVal = retVal && CheckProbes(LevelUpPopupBorderProbePoints, PopUpBorderColor);
				}

				//Tasto sinistro ("Share", verde)
				retVal = retVal && CheckProbes(LevelUpLeftButtonProbePoints, LevelUpGreenButtonColor);

				//Tasto destro ("Cancel", rosso)
				retVal = retVal && CheckProbes(LevelUpRightButtonProbePoints, LevelUpRedButtonColor);

				return retVal;
			}

			#endregion

			/// <summary>
			/// Check if a given set of points are of the given color.
			/// </summary>
			/// <param name="probes">Point to check.</param>
			/// <param name="color">Color to match.</param>
			/// <returns><c>true</c> if all probes are of the given color.</returns>
			private bool CheckProbes(Point[] probes, Color color) {
				bool retVal;
				Color[] probedColors;

				retVal = true;

				probedColors = GetPixelsColors(probes);

				for (int i = 0; i < probedColors.Length && retVal; i++) {
					retVal = retVal && probedColors[i].Equals(color);
				}

				return retVal;
			}

			/// <summary>
			/// Get the color of a series of pixels.
			/// </summary>
			/// <param name="locations">Series of pixels to probe.</param>
			/// <returns></returns>
			private Color[] GetPixelsColors(Point[] locations) {
				Color[] retVal;
				Point topLeft;
				Point botRight;
				Size totalSize;
				Bitmap img;
				Graphics g;

				topLeft = new Point(locations[0].X, locations[0].Y);
				botRight = new Point(locations[0].X, locations[0].Y);

				for (int i = 1; i < locations.Length; i++) {
					if (topLeft.X > locations[i].X) {
						topLeft.X = locations[i].X;
					}

					if (topLeft.Y > locations[i].Y) {
						topLeft.Y = locations[i].Y;
					}

					if (botRight.X < locations[i].X) {
						botRight.X = locations[i].X;
					}

					if (botRight.Y < locations[i].Y) {
						botRight.Y = locations[i].Y;
					}
				}

				totalSize = new Size((botRight.X - topLeft.X) + 1, (botRight.Y - topLeft.Y) + 1);

				if (totalSize.Width * totalSize.Height <= 120000) {
					//Capture screen once for all
					img = new Bitmap(totalSize.Width, totalSize.Height);
					g = Graphics.FromImage(img);
					g.CopyFromScreen(topLeft + midMod, new Point(0, 0), totalSize);

					retVal = new Color[locations.Length];
					for (int i = 0; i < locations.Length; i++) {
						retVal[i] = img.GetPixel(locations[i].X - topLeft.X, locations[i].Y - topLeft.Y);
					}
				} else {
					//Capture screen one pixel at time

					img = new Bitmap(1, 1);
					g = Graphics.FromImage(img);

					retVal = new Color[locations.Length];

					for (int i = 0; i < locations.Length; i++) {
						g.CopyFromScreen(locations[i] + midMod, new Point(0, 0), new Size(1, 1));
						retVal[i] = img.GetPixel(0, 0);
					}
				}

				return retVal;
			}

			/// <summary>
			/// Get the median point of a set of points.
			/// </summary>
			/// <param name="points"></param>
			/// <returns></returns>
			private Point GetMedianPoint(Point[] points) {
				Point retVal;

				retVal = new Point(0,0);

				for (int i = 0; i < points.Length; i++) {
					retVal.X += points[i].X;
					retVal.Y += points[i].Y;
				}

				retVal.X /= points.Length;
				retVal.Y /= points.Length;

				return retVal;
			}

			#endregion

			/// <summary>
			/// Choose a Market Item.
			/// </summary>
			/// <param name="marketType">Market type, i.e. <c>MARKET_TYPE_SEEDS</c>.</param>
			/// <param name="marketPage">Market page, starting from 1.</param>
			/// <param name="marketItem">Market item (1-8).</param>
			protected void ChooseMarketItem(int marketType, int marketPage, int marketItem, bool useHarvester) {

				if (useHarvester) {
					//Select Seeder
					SelectSubTool(TOOL_PLOW, SUBTOOL_PLOW_SEEDER);

					//Wait for the market to open
					Wait(DELAY_STD_OP);
				} else {
					//Open market
					ClickAndWait(ToolPlace(TOOL_MARKET), DELAY_STD_OP);

					//Choose market type
					ClickAndWait(MarketTypePlace(marketType), DELAY_STD_OP);

					if (marketType == MARKET_TYPE_SEEDS_TREES) {
						//HACK: If is the seed&tree page, chose subpage type "Seed"

						//HACK: This button won't work with standard click...
						MoveAndWait(MarketSeedSubPagePlace, DELAY_FAST);
						MouseSimulator.MouseDown(MouseButton.Left);
						Wait(DELAY_FAST);
						MouseSimulator.MouseUp(MouseButton.Left);
						Wait(DELAY_FAST * 3);
					}
				}

				//Choose right page
				MouseSimulator.X = MarketNextPagePlace.X;
				MouseSimulator.Y = MarketNextPagePlace.Y;

				for (int k = 1; k < marketPage; k++) {
					MouseSimulator.Click(MouseButton.Left);

					System.Threading.Thread.Sleep(DELAY_FAST * 2);
				}

				//Choose item
				ClickAt(MarketItemPlace(marketItem - 1));
			}

			/// <summary>
			/// Wait given amount of time.
			/// </summary>
			/// <param name="waitTime">Time to wait to, in milliseconds.</param>
			protected void Wait(int waitTime) {
				System.Threading.Thread.Sleep(waitTime);
			}

			/// <summary>
			/// Move to a given position and wait given amount of time.
			/// </summary>
			/// <param name="position">Position to click at.</param>
			/// <param name="waitTime">Time to wait to, in milliseconds.</param>
			protected void MoveAndWait(Point position, int waitTime) {
				MouseSimulator.X = position.X;
				MouseSimulator.Y = position.Y;

				Wait(waitTime);
			}

			/// <summary>
			/// Click at a given position and wait given amount of time.
			/// </summary>
			/// <param name="position">Position to click at.</param>
			/// <param name="waitTime">Time to wait to, in milliseconds.</param>
			protected void ClickAndWait(Point position, int waitTime) {
				ClickAt(position);

				Wait(waitTime);
			}

			/// <summary>
			/// Click at a given position.
			/// </summary>
			/// <param name="position">Position to click at.</param>
			protected void ClickAt(Point position) {
				Point myPosition;

				myPosition = position;
				//myPosition = ConvertPosition(position);

				MouseSimulator.X = myPosition.X;
				MouseSimulator.Y = myPosition.Y;

				MouseSimulator.Click(MouseButton.Left);
			}

			/// <summary>
			/// Get the offset of a tile.
			/// </summary>
			/// <param name="tileIndex">Tile index.</param>
			/// <param name="offsetRight">Offset for the right tile.</param>
			/// <param name="offsetLeft">Offset for the left tile.</param>
			/// <returns></returns>
			protected Size GetTileOffset(Point tileIndex, Size offsetRight, Size offsetLeft) {
				return new Size(
					(offsetRight.Width * tileIndex.X) + (offsetLeft.Width * tileIndex.Y),
					(offsetRight.Height * tileIndex.X) + (offsetLeft.Height * tileIndex.Y));
			}

			/// <summary>
			/// Get the offset of a tile.
			/// </summary>
			/// <param name="tileIndex">Tile index.</param>
			/// <param name="offsetRight">Offset for the right tile.</param>
			/// <param name="offsetLeft">Offset for the left tile.</param>
			/// <param name="subOffsetLeft">Thousandth of offset for the right tile.</param>
			/// <param name="subOffsetRight">Thousandth of offset for the left tile.</param>
			/// <returns></returns>
			protected Size GetTileOffset(Point tileIndex, Size offsetRight, Size offsetLeft, Size subOffsetRight, Size subOffsetLeft) {
				return new Size(
					(offsetRight.Width * tileIndex.X) + (int)(subOffsetRight.Width * tileIndex.X * 0.001) + (offsetLeft.Width * tileIndex.Y) + (int)(subOffsetLeft.Width * tileIndex.Y * 0.001),
					(offsetRight.Height * tileIndex.X) + (int)(subOffsetRight.Height * tileIndex.X * 0.001) + (offsetLeft.Height * tileIndex.Y) + (int)(subOffsetLeft.Height * tileIndex.Y * 0.001));
			}

			/// <summary>
			/// Seleziona uno strumento cliccando direttamente sulla radice
			/// </summary>
			/// <param name="tool"></param>
			protected void SelectTool(int tool) {
				ClickAndWait(ToolPlace(tool), DELAY_FAST);
			}

			/// <summary>
			/// Seleziona uno strumento passando sulla radice e cliccando il sottostrumento
			/// </summary>
			/// <param name="tool"></param>
			/// <param name="subTool"></param>
			protected void SelectSubTool(int tool, int subTool) {
				MoveAndWait(ToolPlace(tool), DELAY_FAST);
				ClickAndWait(ToolSubPlace(tool, subTool), DELAY_FAST);
			}

			#region Procedure errate di conversione coordinate
			///// <summary>
			///// Converte le coordinate considerando la differenza tra il monitor di riferimento al monitor attuale.
			///// </summary>
			///// <param name="position">Coordinate da convertire.</param>
			///// <returns>Coordinate convertite.</returns>
			//private Point ConvertPosition(Point position) {
			//    int leftOffset;

			//    leftOffset = (actRes.Width - ConvertCoordinate(refRes.Width)) / 2;

			//    return new Point(
			//        ConvertCoordinate(position.X) + leftOffset,
			//        ConvertCoordinate(position.Y));
			//}

			///// <summary>
			///// Converte una coordinata.
			///// </summary>
			///// <param name="coordinate">Coordinata da convertire.</param>
			///// <returns>Coordinata convertita</returns>
			///// <remarks>
			///// La scala è data da "actRes.Height / refRes.Height".<br />
			///// Non metto questo valore in una variabile così posso 
			///// lavorare solo con interi senza approssimare troppo.
			///// </remarks>
			//private int ConvertCoordinate(int coordinate) {
			//    //TODO: Considerare il caso di monitor verticali.
			//    return (coordinate * actRes.Height) / refRes.Height;
			//}
			#endregion

			#endregion
		}

		public class RoboFarmer : BaseFarmer {

			/// <summary>
			/// Width is for NO->SE (\) direction, Height for NE->SO (/) direction.
			/// </summary>
			public Size FieldSize = new Size(1, 1);
			public int ToolSize = 1;

			public Point[] Exclude;

			public bool DoHarvest;
			public bool DoPlow;
			public bool DoSeed;

			bool WaitPrevOp;

			public int SeedPage;
			public int SeedNumber;

			void SetNextStatus() {
				if (DoHarvest && Status < 1) { //Start Harvesting
					Status = 1;
					SubStatus = 0;
					WaitPrevOp = false;
				} else if (DoPlow && Status < 2) { //Start Plowing
					Status = 2;
					SubStatus = 0;
					WaitPrevOp = DoHarvest;
				} else if (DoSeed && Status < 3) { //Select seed
					Status = 3;
					SubStatus = 0;
					WaitPrevOp = DoHarvest || DoPlow;
				} else if (DoSeed && Status < 4) { //Start seeding
					Status = 4;
					SubStatus = 0;
					WaitPrevOp = DoHarvest || DoPlow;
				} else if (!DoHarvest && !DoPlow && !DoSeed && Status < 4) { //No op selected. Start clicking with current tool
					Status = 4;
					SubStatus = 0;
					WaitPrevOp = DoHarvest || DoPlow;
				} else {
					Status = 99;
					SubStatus = 0;
					WaitPrevOp = false;
					Stop();
				}
			}

			protected override void MyTimer_Tick(object sender, EventArgs e) {
				Point currTile;
				bool skip;

				MyTimer.Stop();

				switch (Status) {
					case 0:
						//Inizio il lavoro.
						SetNextStatus();

						MyTimer.Interval = DELAY_NULL;
						MyTimer.Start();
						break;
					case 1:
					case 2:
					case 4:
						//Harvest, plow, put seed

						if (Status == 1 && SubStatus == 0) {
							if (DismissPopup()) {
								//Select harvest tool (cursor)
								//ClickAndWait(ToolPlace(TOOL_CURSOR), DELAY_FAST);
								if (ToolSize > 1) {
									//Extra size -> Select Harvester
									SelectSubTool(TOOL_PLOW, SUBTOOL_PLOW_HARVESTER);
								} else {
									SelectSubTool(TOOL_CURSOR, SUBTOOL_CURSOR_CURSOR);
								}
							} else {
								//Cannot dismiss popup. Terminate execution.
								Stop();
								break;
							}
						} else if (Status == 2 && SubStatus == 0) {
							if (DismissPopup()) {
								//Select plow tool
								//ClickAndWait(ToolPlace(TOOL_PLOW), DELAY_FAST);
								if (ToolSize > 1) {
									//Extra size -> Select Tractor
									SelectSubTool(TOOL_PLOW, SUBTOOL_PLOW_TRACTOR);
								} else {
									SelectSubTool(TOOL_PLOW, SUBTOOL_PLOW_PLOW);
								}
							} else {
								//Cannot dismiss popup. Terminate execution.
								Stop();
								break;
							}
						}

						currTile = new Point(
							SubStatus % (FieldSize.Width),
							SubStatus / (FieldSize.Width));
						
						skip = false;

						//If tool size is more than 1x1, skip odd rows
						skip = (currTile.Y % ToolSize != 0);

						//Check if this tile is in the exclude list.
						for (int k = 0; k < Exclude.Length && skip == false; k++) {
							skip = (Exclude[k] == currTile + adj);
						}

						if (!skip) {
							if (DismissPopup()) {
								//ClickAt(StartingPoint + GetTileOffset(currTile, OffsetNextRight, OffsetNextLeft));
								ClickAt(StartingPoint + GetTileOffset(currTile, OffsetNextRight, OffsetNextLeft, SubOffsetNextRight, SubOffsetNextLeft));
							} else {
								//Cannot dismiss popup. Terminate execution.
								Stop();
								break;
							}
						}

						//SubStatus++;
						SubStatus += ToolSize;

						if (SubStatus < (FieldSize.Width * FieldSize.Height)) {
							//Other square
							MyTimer.Interval = skip ? DELAY_NULL : (WaitPrevOp ? DELAY_PREV_OP : DELAY_FAST);
						} else {
							//Next phase
							SetNextStatus();
							MyTimer.Interval = DELAY_STD_OP;
						}

						MyTimer.Start();
						break;
					case 3:
						//Select right seed

						if (DismissPopup()) {
							ChooseMarketItem(MARKET_TYPE_SEEDS_TREES, SeedPage, SeedNumber, (ToolSize > 1));
						} else {
							//Cannot dismiss popup. Terminate execution.
							Stop();
							break;
						}

						//System.Threading.Thread.Sleep(timeDelayOp);

						//Next phase
						SetNextStatus();

						MyTimer.Interval = DELAY_STD_OP;
						MyTimer.Start();
						break;
					default:
						//Non implementato. Fermo tutto.
						Stop();
						break;
				}
			}

		}

		public class HayBaleTrick : BaseFarmer {

			Size TileSize = new Size(4, 4);
			Size TotalSize;

			/// <summary>
			/// Width is for NO->SE (\) direction, Height for NE->SO (/) direction.
			/// </summary>
			public Size FieldSize = new Size(1, 1);

			public int BalePage = 3;
			public int BaleNumber = 5;

			public override void Start() {

				TotalSize = new Size(TileSize.Width * FieldSize.Width, TileSize.Height * FieldSize.Height);

				base.Start();
			}
			
			protected override void MyTimer_Tick(object sender, EventArgs e) {
				Point currTile;

				MyTimer.Stop();

				switch (Status) {
					case 0:
						//Inizio il lavoro.

						Status = 1;

						MyTimer.Interval = DELAY_NULL;
						MyTimer.Start();
						break;
					case 1:
						//Seleziono le balle

						ChooseMarketItem(MARKET_TYPE_DECORATIONS, BalePage, BaleNumber, false);

						//Passo alla fase successiva
						Status = 2;
						SubStatus = 0;

						MyTimer.Interval = DELAY_STD_OP;
						MyTimer.Start();
						break;
					case 2:
						//Metto le balle.

						currTile = new Point(
							SubStatus % (TotalSize.Width),
							SubStatus / (TotalSize.Width));

						ClickAt(StartingPoint + GetTileOffset(currTile, OffsetNextBaleRight, OffsetNextBaleLeft));

						SubStatus++;

						if (SubStatus < (TotalSize.Width * TotalSize.Height)) {
							//Altra balla
							MyTimer.Interval = DELAY_FAST;
						} else {
							//Passo alla fase successiva
							Status = 3;
							SubStatus = 0;
							MyTimer.Interval = DELAY_STD_OP;
						}

						MyTimer.Start();
						break;
					case 3:
						////Select tool
						//ClickAndWait(ToolPlace(TOOL_CURSOR), DELAY_FAST);
						SelectSubTool(TOOL_CURSOR, SUBTOOL_CURSOR_RECYCLE);

						//Passo alla fase successiva
						Status = 4;
						SubStatus = 0;

						MyTimer.Interval = DELAY_STD_OP;
						MyTimer.Start();
						break;
					case 4:
						//Cancello le balle.

						currTile = new Point(
							SubStatus % (TotalSize.Width),
							SubStatus / (TotalSize.Width));

						//Click su balla
						ClickAndWait(
							StartingPoint + OffsetSelectBale + GetTileOffset(currTile, OffsetNextBaleRight, OffsetNextBaleLeft),
							//DELAY_FAST);
							DELAY_PREV_OP);

						////Click su "Sell"
						//ClickAndWait(
						//    StartingPoint + OffsetSelectBale + OffsetSell + GetTileOffset(currTile, OffsetNextBaleRight, OffsetNextBaleLeft),
						//    //DELAY_STD_OP);
						//    DELAY_PREV_OP);

						////Conferma vendita
						//ClickAt(ConfirmSellPlace);

						if (Status == 4 && SubStatus == 0) {
							//Seleziono "Don't Warn on recycle"
							ClickAndWait(CheckBoxDontWarnOnRecycle, DELAY_FAST);
							//Convermo il riciclaggio
							ClickAndWait(ConfirmRecyclePlace, DELAY_PREV_OP);
						}

						SubStatus++;

						if (SubStatus < (TotalSize.Width * TotalSize.Height)) {
							//Altra balla
						} else {
							//Ricomincio il ciclo
							Status = 1;
							SubStatus = 0;
						}

						//MyTimer.Interval = DELAY_STD_OP; //Dò un pò di tempo extra al "sell"
						MyTimer.Interval = DELAY_NULL;
						MyTimer.Start();
						break;
					default:
						//Non implementato. Fermo tutto.
						Stop();
						break;
				}
			}
		}

		public class PlowSeedDelete : BaseFarmer {

			private Size plowOffset = new Size(0, -3);

			/// <summary>
			/// Width is for NO->SE (\) direction, Height for NE->SO (/) direction.
			/// </summary>
			public Size FieldSize = new Size(1, 1);

			public int SoyPage = 1;
			public int SoyNumber = 3;

			public override void Start() {

				base.Start();
			}

			protected override void MyTimer_Tick(object sender, EventArgs e) {
				Point currTile;

				MyTimer.Stop();

				switch (Status) {
					case 0:
						//Inizio il lavoro.

						Status = 1;

						MyTimer.Interval = DELAY_NULL;
						MyTimer.Start();
						break;
					case 1:
						//Plow

						if (Status == 1 && SubStatus == 0) {
							//Select plow
							//ClickAndWait(ToolPlace(TOOL_PLOW), DELAY_FAST);
							SelectSubTool(TOOL_PLOW, SUBTOOL_PLOW_PLOW);
						}

						currTile = new Point(
							SubStatus % (FieldSize.Width),
							SubStatus / (FieldSize.Width));

						//ClickAt(StartingPoint + GetTileOffset(currTile, OffsetNextRight, OffsetNextLeft));
						ClickAt(StartingPoint + GetTileOffset(currTile, OffsetNextRight, OffsetNextLeft, SubOffsetNextRight, SubOffsetNextLeft));

						SubStatus++;

						if (SubStatus < (FieldSize.Width * FieldSize.Height)) {
							//Other square
							MyTimer.Interval = DELAY_FAST;
						} else {
							//Next phase
							Status = 2;
							SubStatus = 0;
							MyTimer.Interval = DELAY_PREV_OP;
						}

						MyTimer.Start();
						break;
					case 2:
						//Select "SoyBeans"
						ChooseMarketItem(MARKET_TYPE_SEEDS_TREES, SoyPage, SoyNumber, false);

						Status = 3;
						SubStatus = 0;
						MyTimer.Interval = DELAY_PREV_OP;

						MyTimer.Start();
						break;
					case 3:
						//Seed

						currTile = new Point(
							SubStatus % (FieldSize.Width),
							SubStatus / (FieldSize.Width));

						//ClickAt(StartingPoint + GetTileOffset(currTile, OffsetNextRight, OffsetNextLeft));
						ClickAt(StartingPoint + plowOffset + GetTileOffset(currTile, OffsetNextRight, OffsetNextLeft, SubOffsetNextRight, SubOffsetNextLeft));

						SubStatus++;

						if (SubStatus < (FieldSize.Width * FieldSize.Height)) {
							//Other square
							MyTimer.Interval = DELAY_PREV_OP;
						} else {
							//Next phase
							Status = 4;
							SubStatus = 0;
							MyTimer.Interval = DELAY_PREV_OP;
						}

						MyTimer.Start();
						break;
					case 4:
						//Delete.

						if (Status == 4 && SubStatus == 0) {
							//Select Delete Tool
							//MoveAndWait(ToolPlace(TOOL_CURSOR), DELAY_FAST);
							//ClickAndWait(ToolPlace(TOOL_DELETE), DELAY_FAST);
							SelectSubTool(TOOL_CURSOR, SUBTOOL_CURSOR_RECYCLE);
						}

						currTile = new Point(
							SubStatus % (FieldSize.Width),
							SubStatus / (FieldSize.Width));

						//Click su casella (delete)
						//ClickAndWait(StartingPoint + GetTileOffset(currTile, OffsetNextRight, OffsetNextLeft), DELAY_PREV_OP);
						ClickAndWait(StartingPoint + plowOffset + GetTileOffset(currTile, OffsetNextRight, OffsetNextLeft, SubOffsetNextRight, SubOffsetNextLeft),
							DELAY_PREV_OP);

						////Conferma cancellazione
						//ClickAt(ConfirmDeletePlace);
						if (Status == 4 && SubStatus == 0) {
							//Seleziono "Don't Warn on recycle"
							ClickAndWait(CheckBoxDontWarnOnRecycle, DELAY_FAST);
							//Convermo il riciclaggio
							ClickAndWait(ConfirmRecyclePlace, DELAY_PREV_OP);
						}

						SubStatus++;

						if (SubStatus < (FieldSize.Width * FieldSize.Height)) {
							//Altra casella
						} else {
							//Ricomincio il ciclo
							Status = 1;
							SubStatus = 0;
						}

						//MyTimer.Interval = DELAY_STD_OP; //Dò un pò di tempo extra al "delete"
						MyTimer.Interval = DELAY_PREV_OP;
						MyTimer.Start();
						break;
					default:
						//Non implementato. Fermo tutto.
						Stop();
						break;
				}
			}
		}

		enum ToolType {
			/// <summary>
			/// Waiting (no tool).
			/// </summary>
			Wait = 0,
			/// <summary>
			/// Hay Bale Trick
			/// </summary>
			Bale = 1,
			/// <summary>
			/// Robot Farmer.
			/// </summary>
			Farmer = 2,
			/// <summary>
			/// Plow, Seed, Delete Trick
			/// </summary>
			PSD = 3
		}

		private RoboFarmer farmer;
		private HayBaleTrick tricker;
		private PlowSeedDelete soyTricker;

		private RadioButton[] rbSeedPage;
		private RadioButton[] rbBalePage;
		private RadioButton[] rbSoyBPage;

		Rectangle seedSpace = new Rectangle();
		Rectangle baleSpace = new Rectangle();
		Rectangle soySpace = new Rectangle();

		private ToolType mainStatus = ToolType.Wait;

		public FarmVilleTrainer() {
			InitializeComponent();

			rbSeedPage = new RadioButton[BaseFarmer.SEED_PER_PAGE];
			rbBalePage = new RadioButton[BaseFarmer.SEED_PER_PAGE];
			rbSoyBPage = new RadioButton[BaseFarmer.SEED_PER_PAGE];

			this.SuspendLayout();

			seedSpace = new Rectangle();
			seedSpace.X = 0;
			seedSpace.Y = nudSeedPage.Location.Y + nudSeedPage.Size.Height + 3;
			seedSpace.Width = gbSeeds.ClientSize.Width / BaseFarmer.SEED_COLUMN;
			seedSpace.Height = (gbSeeds.ClientSize.Height - (seedSpace.Top + 3)) / (BaseFarmer.SEED_PER_PAGE / BaseFarmer.SEED_COLUMN);

			baleSpace = new Rectangle();
			baleSpace.X = 0;
			baleSpace.Y = nudHayBalePage.Location.Y + nudHayBalePage.Size.Height + 3;
			baleSpace.Width = gbHayBale.ClientSize.Width / BaseFarmer.SEED_COLUMN;
			baleSpace.Height = (gbHayBale.ClientSize.Height - (baleSpace.Top + 3)) / (BaseFarmer.SEED_PER_PAGE / BaseFarmer.SEED_COLUMN);

			soySpace = new Rectangle();
			soySpace.X = 0;
			soySpace.Y = nudSoyBPage.Location.Y + nudSoyBPage.Size.Height + 3;
			soySpace.Width = gbSoyBeans.ClientSize.Width / BaseFarmer.SEED_COLUMN;
			soySpace.Height = (gbSoyBeans.ClientSize.Height - (soySpace.Top + 3)) / (BaseFarmer.SEED_PER_PAGE / BaseFarmer.SEED_COLUMN);

			for (int i = 0; i < rbSeedPage.Length; i++) {
				rbSeedPage[i] = new RadioButton();
				rbSeedPage[i].AutoSize = true;
				rbSeedPage[i].Name = string.Format("rbSeedPage{0:00}", i);
				rbSeedPage[i].Text = null;
				rbSeedPage[i].Margin = new Padding(3);
				rbSeedPage[i].Checked = i == 0;
				rbSeedPage[i].UseVisualStyleBackColor = true;
				
				gbSeeds.Controls.Add(rbSeedPage[i]);

				rbSeedPage[i].Location = new Point(
					seedSpace.Left + (seedSpace.Width - rbSeedPage[i].Size.Width) / 2 + seedSpace.Width * (i % BaseFarmer.SEED_COLUMN),
					seedSpace.Top + (seedSpace.Height - rbSeedPage[i].Size.Height) / 2 + seedSpace.Height * (i / BaseFarmer.SEED_COLUMN));

				rbBalePage[i] = new RadioButton();
				rbBalePage[i].AutoSize = true;
				rbBalePage[i].Name = string.Format("rbBalePage{0:00}", i);
				rbBalePage[i].Text = string.Empty;
				rbBalePage[i].Checked = i == 0;
				rbBalePage[i].Location = new Point(9 + 20 * (i % BaseFarmer.SEED_COLUMN), 44 + 19 * (i / BaseFarmer.SEED_COLUMN));
				rbBalePage[i].UseVisualStyleBackColor = true;

				gbHayBale.Controls.Add(rbBalePage[i]);

				rbBalePage[i].Location = new Point(
					baleSpace.Left + (baleSpace.Width - rbBalePage[i].Size.Width) / 2 + baleSpace.Width * (i % BaseFarmer.SEED_COLUMN),
					baleSpace.Top + (baleSpace.Height - rbBalePage[i].Size.Height) / 2 + baleSpace.Height * (i / BaseFarmer.SEED_COLUMN));

				rbSoyBPage[i] = new RadioButton();
				rbSoyBPage[i].AutoSize = true;
				rbSoyBPage[i].Name = string.Format("rbSoyBPage{0:00}", i);
				rbSoyBPage[i].Text = string.Empty;
				rbSoyBPage[i].Checked = i == 0;
				rbSoyBPage[i].Location = new Point(9 + 20 * (i % BaseFarmer.SEED_COLUMN), 44 + 19 * (i / BaseFarmer.SEED_COLUMN));
				//rbSoyBPage[i].Size = new Size(14, 13);
				rbSoyBPage[i].UseVisualStyleBackColor = true;

				gbSoyBeans.Controls.Add(rbSoyBPage[i]);

				rbSoyBPage[i].Location = new Point(
					soySpace.Left + (soySpace.Width - rbSoyBPage[i].Size.Width) / 2 + soySpace.Width * (i % BaseFarmer.SEED_COLUMN),
					soySpace.Top + (soySpace.Height - rbSoyBPage[i].Size.Height) / 2 + soySpace.Height * (i / BaseFarmer.SEED_COLUMN));
			}

			cbToolSize.SelectedIndex = 0;

			this.ResumeLayout();
		}

		private void FarmVilleTrainer_Load(object sender, EventArgs e) {
			farmer = new RoboFarmer();
			tricker = new HayBaleTrick();
			soyTricker = new PlowSeedDelete();

			gbSeeds.Enabled = cbSeed.Checked;
		}

		private void cmdExit_Click(object sender, EventArgs e) {
			exitAll();
		}

		private void cmdStartBale_Click(object sender, EventArgs e) {
			if (mainStatus == ToolType.Wait) {

				#region Load form data to "tricker"

				//if (MessageBox.Show(this, "Activate Hay Bale Trick.\nAre you sure?", "FarmVille Trainer", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
				tricker.FieldSize.Width = (int)nudHayBaleWidth.Value;
				tricker.FieldSize.Height = (int)nudHayBaleHeight.Value;

				tricker.BalePage = (int)nudHayBalePage.Value;
				for (int i = 0; i < rbBalePage.Length; i++) {
					if (rbBalePage[i].Checked) {
						tricker.BaleNumber = i + 1;
						break;
					}
				}

				#endregion

				doStartTrick();
				//}
			} else {
				doStopTrick();
			}
		}

		private void cmdStartFarmer_Click(object sender, EventArgs e) {
			if (mainStatus == ToolType.Wait) {

				#region Load form data to "farmer"

				FixNudValue(nudHeight);
				FixNudValue(nudWidth);

				farmer.FieldSize.Width = (int)nudWidth.Value;
				farmer.FieldSize.Height = (int)nudHeight.Value;
				farmer.ToolSize = cbToolSize.SelectedIndex + 1;

				if (txtExclusion.Text.Length > 0) {
					try {
						string[] tmp;
						string[] tmp2;
						tmp = txtExclusion.Text.Split('|');
						farmer.Exclude = new Point[tmp.Length];
						for (int i = 0; i < tmp.Length; i++) {
							tmp2 = tmp[i].Split(',');
							farmer.Exclude[i] = new Point(int.Parse(tmp2[0]), int.Parse(tmp2[1]));
						}
					} catch (Exception) {
						farmer.Exclude = new Point[0];
					}
				} else {
					farmer.Exclude = new Point[0];
				}

				farmer.DoHarvest = cbHarvest.Checked;
				farmer.DoPlow = cbPlow.Checked;
				farmer.DoSeed = cbSeed.Checked;

				if (farmer.DoSeed) {
					farmer.SeedPage = (int)nudSeedPage.Value;
					for (int i = 0; i < rbSeedPage.Length; i++) {
						if (rbSeedPage[i].Checked) {
							farmer.SeedNumber = i + 1;
							break;
						}
					}
				}

				#endregion

				doStartFarm();
			} else {
				doStopFarm();
			}
		}

		private void cmdStartPSD_Click(object sender, EventArgs e) {
			if (mainStatus == ToolType.Wait) {

				#region Load form data to "soyTricker"

				soyTricker.FieldSize.Width = (int)nudPSDWidth.Value;
				soyTricker.FieldSize.Height = (int)nudPSDHeight.Value;

				soyTricker.SoyPage = (int)nudSoyBPage.Value;
				for (int i = 0; i < rbSoyBPage.Length; i++) {
					if (rbSoyBPage[i].Checked) {
						soyTricker.SoyNumber = i + 1;
						break;
					}
				}

				#endregion

				doStartSoyTrick();
			} else {
				doStopSoyTrick();
			}
		}

		private void cbSeed_CheckedChanged(object sender, EventArgs e) {
			gbSeeds.Enabled = cbSeed.Checked;
		}

		private void FarmVilleTrainer_Activated(object sender, EventArgs e) {
			if (mainStatus == ToolType.Bale) {
				doStopTrick();
			} else if (mainStatus == ToolType.Farmer) {
				doStopFarm();
			} else if (mainStatus == ToolType.PSD) {
				doStopSoyTrick();
			}
		}

		private void cmdAbout_Click(object sender, EventArgs e) {
			AboutBox ab;
			ab = new AboutBox();
			ab.ShowDialog(this);
		}

		private void cbToolSize_SelectedIndexChanged(object sender, EventArgs e) {
			int toolSize;
			toolSize = ((ComboBox)sender).SelectedIndex + 1;

			nudHeight.Increment = toolSize;
			FixNudValue(nudHeight);

			nudWidth.Increment = toolSize;
			FixNudValue(nudWidth);
		}

		private void NumericUpDown_Leave(object sender, EventArgs e) {
			FixNudValue((NumericUpDown)sender);
		}

		#region Metodi privati/protetti

		private void disableLayout(ToolType request) {
			this.SuspendLayout();

			foreach (TabPage tabPage in tabControlMain.TabPages) {
				foreach (Control ctrl in tabPage.Controls) {
					ctrl.Enabled = false;
				}
			}
			cmdStartFarmer.Text = "Stop";
			cmdStartBale.Text = "Stop";
			cmdStartPSD.Text = "Stop";
			cmdStartFarmer.Enabled = true;
			cmdStartBale.Enabled = true;
			cmdStartPSD.Enabled = true;

			this.ResumeLayout();
		}

		private void enableLayout(ToolType request) {
			this.SuspendLayout();

			foreach (TabPage tabPage in tabControlMain.TabPages) {
				foreach (Control ctrl in tabPage.Controls) {
					ctrl.Enabled = true;
				}
			}

			cmdStartFarmer.Text = "Start";
			cmdStartBale.Text = "Start";
			cmdStartPSD.Text = "Start";

			gbSeeds.Enabled = cbSeed.Checked;

			this.ResumeLayout();
		}

		private void exitAll() {
			this.Close();
		}

		private void doStartTrick() {
			disableLayout(ToolType.Bale);

			mainStatus = ToolType.Bale;

			tricker.Start();
		}

		private void doStopTrick() {
			tricker.Stop();

			mainStatus = ToolType.Wait;

			enableLayout(ToolType.Bale);
		}

		private void doStartFarm() {
			disableLayout(ToolType.Farmer);

			mainStatus = ToolType.Farmer;

			farmer.Start();
		}

		private void doStopFarm() {
			farmer.Stop();

			mainStatus = ToolType.Wait;

			enableLayout(ToolType.Farmer);
		}

		private void doStartSoyTrick() {
			disableLayout(ToolType.PSD);

			mainStatus = ToolType.PSD;

			soyTricker.Start();
		}

		private void doStopSoyTrick() {
			soyTricker.Stop();

			mainStatus = ToolType.Wait;

			enableLayout(ToolType.PSD);
		}

		private void FixNudValue(NumericUpDown nud) {
			decimal newVal;
			if (nud.Value % nud.Increment != 0) {
				newVal = nud.Value + nud.Increment - (nud.Value % nud.Increment);
				if (newVal > nud.Maximum) {
					newVal -= nud.Increment;
				}
				nud.Value = newVal;
			}
		}

		#endregion

	}
}