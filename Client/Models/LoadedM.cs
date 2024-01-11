using static BlazorD.Client.Models.Cell;
using System.Drawing;

namespace BlazorD.Client.Models {
    public class LoadedM {
        public int W { get { return w; } }
        public int H { get { return h; } }
        public List<List<Cell>> actualMap = new List<List<Cell>>();

        private static List<string> lvlT = new List<string>() { "E", "D", "C", "B", "A", "S" };
        public static List<string> LvlT { get { return lvlT; } }

        List<Point> r = new List<Point>();

        public int status = 0;

        int w = 0, h = 0;

        public LoadedM(List<string> lm) {
            List<Cell> row = null;
            for (int i = 0; i < lm.Count; i++) {
                row = new List<Cell>();
                string[] vs = lm[i].Split('\t');
                for (int j = 0; j < vs.Length; j++) {
                    Cell cell = new Cell(Cell.CellType.Empty, 0);
                    if (vs[j] == "/") cell = new Cell(Cell.CellType.Wall, 0);
                    else if (vs[j] == "P") cell = new Cell(Cell.CellType.Player, 0);
                    else if (lvlT.Contains(vs[j])) cell = new Cell(Cell.CellType.Chest, lvlT.IndexOf(vs[j]) + 1);
                    else if (vs[j].Length == 2 && lvlT.Contains(vs[j][0].ToString()) && vs[j][1] == 'H') cell = new Cell(Cell.CellType.HiddenChest, lvlT.IndexOf(vs[j][0].ToString()) + 1);
                    else if (vs[j][0] > '0' && vs[j][0] <= '9') cell = new Cell(Cell.CellType.Monster, int.Parse(vs[j][0].ToString()));
                    else if (vs[j].Length == 2 && vs[j][0] == 'H' && vs[j][1] == 'S') cell = new Cell(Cell.CellType.HiddenStairs, 0);
                    else if (vs[j] == "!") cell = new Cell(Cell.CellType.Stairs, 0);
                    row.Add(cell);
                }
                actualMap.Add(row);
            }
            h = actualMap.Count;
            if (h == 0) { status = 2; return; } else { status = 1; w = actualMap[0].Count; }

            int f = 0;
            for (int i = 0; i < actualMap.Count; i++) {
                for (int j = 0; j < actualMap[i].Count; j++) {
                    if (actualMap[i][j].Type == Cell.CellType.Player) f++;
                }
            }
            if (f != 1 || status == 0) { status = 2; return; }
        }

        public LoadedM(List<List<string>> lm) {
            List<Cell> row = null;
            for (int i = 0; i < lm.Count; i++) {
                row = new List<Cell>();
                for (int j = 0; j < lm[i].Count; j++) {
                    Cell cell = new Cell(Cell.CellType.Empty, 0);
                    if (lm[i][j] == "/") cell = new Cell(Cell.CellType.Wall, 0);
                    else if (lm[i][j] == "P") cell = new Cell(Cell.CellType.Player, 0);
                    else if (lvlT.Contains(lm[i][j])) cell = new Cell(Cell.CellType.Chest, lvlT.IndexOf(lm[i][j]) + 1);
                    else if (lm[i][j].Length == 2 && lvlT.Contains(lm[i][j][0].ToString()) && lm[i][j][1] == 'H') cell = new Cell(Cell.CellType.HiddenChest, lvlT.IndexOf(lm[i][j][0].ToString()) + 1);
                    else if (lm[i][j][0] > '0' && lm[i][j][0] <= '9') cell = new Cell(Cell.CellType.Monster, int.Parse(lm[i][j][0].ToString()));
                    else if (lm[i][j].Length == 2 && lm[i][j][0] == 'H' && lm[i][j][1] == 'S') cell = new Cell(Cell.CellType.HiddenStairs, 0);
                    else if (lm[i][j] == "!") cell = new Cell(Cell.CellType.Stairs, 0);
                    row.Add(cell);
                }
                actualMap.Add(row);
            }
            h = actualMap.Count;
            if (h == 0) { status = 2; return; } else { status = 1; w = actualMap[0].Count; }

            int f = 0;
            for (int i = 0; i < actualMap.Count; i++) {
                for (int j = 0; j < actualMap[i].Count; j++) {
                    if (actualMap[i][j].Type == Cell.CellType.Player) f++;
                    if (f > 1) { status = 2; return; }
                }
            }
            if (f != 1 || status == 0) { status = 2; return; }
        }

        public Point GetP() {
            for (int i = 0; i < actualMap.Count; i++) { for (int j = 0; j < actualMap[i].Count; j++) { if (actualMap[i][j].Type == Cell.CellType.Player) return new Point(j, i); } }
            return new Point(-1, -1);
        }

        public Dictionary<Point, Tuple<Cell, bool>> GetSurroundings() {
            Dictionary<Point, Tuple<Cell, bool>> ret = new Dictionary<Point, Tuple<Cell, bool>>();
            Point p = GetP();

            if (!IsOutBounds(p.X, p.Y - 1)) ret.Add(new Point(p.X, p.Y - 1), new Tuple<Cell, bool>(actualMap[p.Y - 1][p.X], IsRevealed(p.X, p.Y - 1)));
            if (!IsOutBounds(p.X, p.Y + 1)) ret.Add(new Point(p.X, p.Y + 1), new Tuple<Cell, bool>(actualMap[p.Y + 1][p.X], IsRevealed(p.X, p.Y + 1)));
            if (!IsOutBounds(p.X - 1, p.Y)) ret.Add(new Point(p.X - 1, p.Y), new Tuple<Cell, bool>(actualMap[p.Y][p.X - 1], IsRevealed(p.X - 1, p.Y)));
            if (!IsOutBounds(p.X + 1, p.Y)) ret.Add(new Point(p.X + 1, p.Y), new Tuple<Cell, bool>(actualMap[p.Y][p.X + 1], IsRevealed(p.X + 1, p.Y)));

            if (!IsOutBounds(p.X - 1, p.Y - 1)) ret.Add(new Point(p.X - 1, p.Y - 1), new Tuple<Cell, bool>(actualMap[p.Y - 1][p.X - 1], IsRevealed(p.X - 1, p.Y - 1)));
            if (!IsOutBounds(p.X + 1, p.Y + 1)) ret.Add(new Point(p.X + 1, p.Y + 1), new Tuple<Cell, bool>(actualMap[p.Y + 1][p.X + 1], IsRevealed(p.X + 1, p.Y + 1)));
            if (!IsOutBounds(p.X - 1, p.Y + 1)) ret.Add(new Point(p.X - 1, p.Y + 1), new Tuple<Cell, bool>(actualMap[p.Y + 1][p.X - 1], IsRevealed(p.X - 1, p.Y + 1)));
            if (!IsOutBounds(p.X + 1, p.Y - 1)) ret.Add(new Point(p.X + 1, p.Y - 1), new Tuple<Cell, bool>(actualMap[p.Y - 1][p.X + 1], IsRevealed(p.X + 1, p.Y - 1)));
            return ret;
        }

        public bool IsRevealed(int x, int y) { return r.Contains(new Point(x, y)); }

        public void RevealFOW(int x, int y) { if (!IsRevealed(x, y)) r.Add(new Point(x, y)); }

        public bool IsOutBounds(int x, int y) { return (y < 0 || y >= h || x < 0 || x >= w); }
        public bool CanMove(int x, int y) {
            if (IsOutBounds(x, y)) return false;
            Cell c = actualMap[y][x];
            if (c.Type == Cell.CellType.Empty || c.Type == Cell.CellType.Chest) return true;
            return c.Type != Cell.CellType.Monster && c.Type != Cell.CellType.Wall;
        }

        public CellType Move(int dir, out Point movedTo) {
            movedTo = new Point(-1, -1);
            Point p = GetP();
            if (dir == 1) { if (CanMove(p.X + 1, p.Y)) { movedTo = new Point(p.X + 1, p.Y); return CheckMoveType(p, new Point(p.X + 1, p.Y)); } } else if (dir == 2) { if (CanMove(p.X - 1, p.Y)) { movedTo = new Point(p.X - 1, p.Y); return CheckMoveType(p, new Point(p.X - 1, p.Y)); } } else if (dir == 3) { if (CanMove(p.X, p.Y - 1)) { movedTo = new Point(p.X, p.Y - 1); return CheckMoveType(p, new Point(p.X, p.Y - 1)); } } else if (dir == 4) { if (CanMove(p.X, p.Y + 1)) { movedTo = new Point(p.X, p.Y + 1); return CheckMoveType(p, new Point(p.X, p.Y + 1)); } }
            return CellType.Empty;
        }

        private CellType CheckMoveType(Point oP, Point nP) {
            Cell c = actualMap[nP.Y][nP.X];
            if (c.Type == Cell.CellType.Stairs || c.Type == Cell.CellType.Chest) return c.Type;
            AbsoluteMove(oP, nP);
            return CellType.Empty;
        }

        public void AbsoluteMove(int dir) {
            Point p = GetP();
            if (dir == 1) { if (CanMove(p.X + 1, p.Y)) { AbsoluteMove(p, new Point(p.X + 1, p.Y)); } } else if (dir == 2) { if (CanMove(p.X - 1, p.Y)) { AbsoluteMove(p, new Point(p.X - 1, p.Y)); } } else if (dir == 3) { if (CanMove(p.X, p.Y - 1)) { AbsoluteMove(p, new Point(p.X, p.Y - 1)); } } else if (dir == 4) { if (CanMove(p.X, p.Y + 1)) { AbsoluteMove(p, new Point(p.X, p.Y + 1)); } }
        }
        private void AbsoluteMove(Point oP, Point nP) { actualMap[oP.Y][oP.X].Type = CellType.Empty; actualMap[nP.Y][nP.X].Type = CellType.Player; }

        //public void Smoke(Item item) {
        //    if (item.Type != Item.ItemType.Smoke) return;
        //    Point p = GetP();

        //    List<Point> circle = new List<Point>();

        //    for (int j = 1; j < 5; j++) // 5 è il raggio
        //    {
        //        for (int i = 0; i < 360; i++) {
        //            double radians = ToRadians(i);
        //            Point cP = new Point((int)Math.Round(p.X + (j * Math.Cos(radians))), (int)Math.Round(p.Y + (j * Math.Sin(radians))));
        //            if (!circle.Contains(cP)) circle.Add(cP);
        //        }
        //    }

        //    for (int i = 0; i < circle.Count; i++) {
        //        if (IsSightClear(circle[i], p, out _)) {
        //            if (!IsOutBounds(circle[i].X, circle[i].Y)) {
        //                m[circle[i].Y][circle[i].X].Smoked = true;
        //                m[circle[i].Y][circle[i].X].SmokeDuration = item.Duration;
        //            }
        //        }
        //    }
        //}

        public void RevealAroundPlayer(int iRange) {
            Point p = GetP();
            List<Point> circle = new List<Point>();

            for (int j = 1; j <= iRange; j++) {
                for (int i = 0; i < 360; i++) {
                    double radians = ToRadians(i);
                    Point cP = new Point((int)Math.Round(p.X + (j * Math.Cos(radians))), (int)Math.Round(p.Y + (j * Math.Sin(radians))));
                    if (!circle.Contains(cP)) circle.Add(cP);
                }
            }

            for (int i = 0; i < circle.Count; i++) {
                if (IsSightClear(circle[i], p, out _, true)) {
                    if (!IsOutBounds(circle[i].X, circle[i].Y)) {
                        RevealFOW(circle[i].X, circle[i].Y);
                    }
                }
            }
        }

        double ToRadians(double i) { return i * (Math.PI / 180); }

        private static List<Point> BresLineOrig(Point begin, Point end) {
            List<Point> passati = new List<Point>();
            int w = end.X - begin.X;
            int h = end.Y - begin.Y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest)) {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++) {
                passati.Add(new Point(begin.X, begin.Y));
                numerator += shortest;
                if (!(numerator < longest)) {
                    numerator -= longest;
                    begin.X += dx1;
                    begin.Y += dy1;
                } else {
                    begin.X += dx2;
                    begin.Y += dy2;
                }
            }
            return passati;
        }

        public bool IsSightClear(Point source, Point target, out List<Point> passati, bool bIncludeWalls) {
            passati = BresLineOrig(source, target);

            if (bIncludeWalls) {
                for (int i = 0; i < passati.Count; i++) { if (IsOutBounds(passati[i].X, passati[i].Y)) return false; }

                int iWalls = 0;
                for (int i = 0; i < passati.Count; i++) { if (actualMap[passati[i].Y][passati[i].X].Type == CellType.Wall) iWalls++; }
                if (iWalls > 1) return false; // Se c'è più di un muro di sicuro non è ok.

                //Se c'è un solo muro ed il source è un muro allora va bene, se bIncludeWalls è true.
                if (iWalls == 1 && actualMap[source.Y][source.X].Type == CellType.Wall) return true;
                return iWalls == 0;
            } else {
                for (int i = 0; i < passati.Count; i++) {
                    if (IsOutBounds(passati[i].X, passati[i].Y) || actualMap[passati[i].Y][passati[i].X].Type == CellType.Wall) return false;
                }
            }
            return true;
        }
    }

    public class Cell {
        public enum CellType { Empty = 0, Wall = 1, Monster = 2, Chest = 3, HiddenChest = 4, HiddenStairs = 5, Stairs = 6, Player = 7 }
        private static List<string> CellTypeColors = new List<string>() { "white", "grey", "red", "yellow", "yellow", "blue", "blue", "green", "black" };

        public CellType Type;
        //public bool Smoked = false;
        //public int SmokeDuration = 0;

        public int CurrentHealth = 0;
        public int Level = 0;

        //public Cell(CellType type, bool smoked, int smokeDuration, int level) {
        //    this.Type = type; this.Smoked = smoked; this.SmokeDuration = smokeDuration; this.Level = level;
        //    if (type == CellType.Monster) CurrentHealth = level * 10;
        //}

        public Cell(CellType type, int level) {
            this.Type = type; this.Level = level;
            if (type == CellType.Monster) CurrentHealth = level * 10;
        }

        public override string ToString() {
            switch (Type) {
                case CellType.Player: return "P";
                case CellType.Stairs: return "!";
                case CellType.Wall: return "/";
                case CellType.Monster: return Level.ToString();
                case CellType.Chest: return LoadedM.LvlT[Math.Min(Level - 1, 0)];
                default: return " ";
            }
        }

        public string ToColor() {
            return CellTypeColors[(int)Type];
        }
    }
}
