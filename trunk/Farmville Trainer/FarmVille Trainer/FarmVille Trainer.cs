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
			protected const int DELAY_FAST = 200;
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
			protected const int SUBTOOL_PLOW_HARVESTER = 1;
			protected const int SUBTOOL_PLOW_SEEDER = 2;
			protected const int SUBTOOL_PLOW_PLANE = 3;
			protected const int SUBTOOL_PLOW_TRACTOR = 4;

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
					new Point(948, 800), //Harvester
					new Point(948, 735), //Seeder
					new Point(948, 670), //Plane
					new Point(948, 600)  //Tractor
				},
				new Point[] {},
				new Point[] {},
				new Point[] {},
				new Point[] {}
			};

			protected const int MARKET_TYPE_SEEDS = 0;
			protected const int MARKET_TYPE_TREES = 1;
			protected const int MARKET_TYPE_ANIMALS = 2;
			protected const int MARKET_TYPE_BUILDINGS = 3;
			protected const int MARKET_TYPE_DECORATIONS = 4;
			protected const int MARKET_TYPE_EXPANDFARM = 5;
			protected const int MARKET_TYPE_VEICHLES = 6;

			protected static Point[] _marketTypePlaces = new Point[] { 
				new Point(328, 328), //Seeds
				new Point(400, 328), //Trees
				new Point(500, 328), //Animals
				new Point(600, 328), //Buildings
				new Point(700, 328), //Decorations
				new Point(800, 328), //Upgrade
				new Point(938, 328)  //Vehicles
			};

			//12/05/2010: Cambio dimensioni da 25, 12 a 20, 9.6
			//04/07/2010: Cambio dimensioni da 20, 9.6 a 15, 7.2
			protected static Size OffsetNextRight = new Size(15, 7);
			protected static Size SubOffsetNextRight = new Size(0, 2); //Decimi da aggiungere a OffsetNextLeft
			protected static Size OffsetNextLeft = new Size(-15, 7);
			protected static Size SubOffsetNextLeft = new Size(0, 2); //Decimi da aggiungere a OffsetNextLeft

			protected static Size OffsetNextBaleRight = new Size(25, 12);
			protected static Size OffsetNextBaleLeft = new Size(-25, 12);

			protected static Size OffsetSell = new Size(32, 32);
			protected static Size OffsetSelectBale = new Size(0, -22);

			/// <summary>
			/// 1x1 size element to adjust zero based points
			/// </summary>
			protected static Size adj = new Size(1, 1);

			protected static Point _marketNextPagePlace = new Point(972, 546);

			//137, 196
			protected static Point[] _marketItemPlaces = new Point[] { 
				new Point(430, 565), new Point(567, 565), new Point(704, 565), new Point(841, 565),
				new Point(430, 761), new Point(567, 761), new Point(704, 761), new Point(841, 761)
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
			/// <param name="subOffsetLeft">Tenth of offset for the right tile.</param>
			/// <param name="subOffsetRight">Tenth of offset for the left tile.</param>
			/// <returns></returns>
			protected Size GetTileOffset(Point tileIndex, Size offsetRight, Size offsetLeft, Size subOffsetRight, Size subOffsetLeft) {
				return new Size(
					(offsetRight.Width * tileIndex.X) + (int)(subOffsetRight.Width * tileIndex.X * 0.1) + (offsetLeft.Width * tileIndex.Y) + (int)(subOffsetLeft.Width * tileIndex.Y * 0.1),
					(offsetRight.Height * tileIndex.X) + (int)(subOffsetRight.Height * tileIndex.X * 0.1) + (offsetLeft.Height * tileIndex.Y) + (int)(subOffsetLeft.Height * tileIndex.Y * 0.1));
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
				if (DoHarvest && Status < 1) {
					Status = 1;
					SubStatus = 0;
					WaitPrevOp = false;
				} else if (DoPlow && Status < 2) {
					Status = 2;
					SubStatus = 0;
					WaitPrevOp = DoHarvest;
				} else if (DoSeed && Status < 3) {
					Status = 3;
					SubStatus = 0;
					WaitPrevOp = DoHarvest || DoPlow;
				} else if (DoSeed && Status < 4) {
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
							//Select harvest tool (cursor)
							//ClickAndWait(ToolPlace(TOOL_CURSOR), DELAY_FAST);
							if (ToolSize > 1) {
								//Extra size -> Select Harvester
								SelectSubTool(TOOL_PLOW, SUBTOOL_PLOW_HARVESTER);
							} else {
								SelectSubTool(TOOL_CURSOR, SUBTOOL_CURSOR_CURSOR);
							}
						} else if (Status == 2 && SubStatus == 0) {
							//Select plow tool
							//ClickAndWait(ToolPlace(TOOL_PLOW), DELAY_FAST);
							if (ToolSize > 1) {
								//Extra size -> Select Tractor
								SelectSubTool(TOOL_PLOW, SUBTOOL_PLOW_TRACTOR);
							} else {
								SelectSubTool(TOOL_PLOW, SUBTOOL_PLOW_PLOW);
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
							//ClickAt(StartingPoint + GetTileOffset(currTile, OffsetNextRight, OffsetNextLeft));
							ClickAt(StartingPoint + GetTileOffset(currTile, OffsetNextRight, OffsetNextLeft, SubOffsetNextRight, SubOffsetNextLeft));
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

						ChooseMarketItem(MARKET_TYPE_SEEDS, SeedPage, SeedNumber, (ToolSize > 1));

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
						ChooseMarketItem(MARKET_TYPE_SEEDS, SoyPage, SoyNumber, false);

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

		private ToolType mainStatus = ToolType.Wait;

		public FarmVilleTrainer() {
			InitializeComponent();

			rbSeedPage = new RadioButton[8];
			rbBalePage = new RadioButton[8];
			rbSoyBPage = new RadioButton[8];

			this.SuspendLayout();

			for (int i = 0; i < rbSeedPage.Length; i++) {
				rbSeedPage[i] = new RadioButton();
				rbSeedPage[i].AutoSize = false;
				rbSeedPage[i].Name = string.Format("rbSeedPage{0:00}", i);
				rbSeedPage[i].Text = string.Empty;
				rbSeedPage[i].Checked = i == 0;
				rbSeedPage[i].Location = new Point(9 + 20 * (i % 4), 44 + 19 * (i / 4));
				rbSeedPage[i].Size = new Size(14, 13);
				rbSeedPage[i].UseVisualStyleBackColor = true;
				
				gbSeeds.Controls.Add(rbSeedPage[i]);

				rbBalePage[i] = new RadioButton();
				rbBalePage[i].AutoSize = false;
				rbBalePage[i].Name = string.Format("rbBalePage{0:00}", i);
				rbBalePage[i].Text = string.Empty;
				rbBalePage[i].Checked = i == 0;
				rbBalePage[i].Location = new Point(9 + 20 * (i % 4), 44 + 19 * (i / 4));
				rbBalePage[i].Size = new Size(14, 13);
				rbBalePage[i].UseVisualStyleBackColor = true;

				gbHayBale.Controls.Add(rbBalePage[i]);

				rbSoyBPage[i] = new RadioButton();
				rbSoyBPage[i].AutoSize = false;
				rbSoyBPage[i].Name = string.Format("rbSoyBPage{0:00}", i);
				rbSoyBPage[i].Text = string.Empty;
				rbSoyBPage[i].Checked = i == 0;
				rbSoyBPage[i].Location = new Point(9 + 20 * (i % 4), 44 + 19 * (i / 4));
				rbSoyBPage[i].Size = new Size(14, 13);
				rbSoyBPage[i].UseVisualStyleBackColor = true;

				gbSoyBeans.Controls.Add(rbSoyBPage[i]);
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