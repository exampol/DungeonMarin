using System.Drawing;
using System.Runtime.InteropServices;

namespace BlazorD.Client.Models {
    class Generation {
        private int size = 0;
        public int Size { get { return size; } }

        private List<List<string>> m = new List<List<string>>();
        public List<List<string>> Map = new List<List<string>>();

        private int numeroStanzeMassimo = 0;
        private int minS = 3;
        private int maxS = 6;

        public int minMonsters = 0;
        public int maxMonsters = 2;

        public int minLevel = 1;
        public int maxLevel = 3;

        private List<string> tLvls = new List<string>() { "E", "D", "C", "B", "A", "S" };
        public int minLoot = 1;
        public int maxLoot = 2;

        private List<Room> rooms = new List<Room>();
        public List<Room> Rooms { get { return rooms; } }

        private List<Corridor> corridors = new List<Corridor>();
        public List<Corridor> Corridors { get { return corridors; } }

        public bool IsOutBounds(int x, int y) { return (y < 0 || y >= size || x < 0 || x >= size); }

        public Generation(int iSize) {
            size = iSize;
            for (int i = 0; i < size; i++) {
                Map.Add(new List<string>());
                if (i == 0 || i == size - 1) {
                    for (int j = 0; j < size; j++) {
                        Map[i].Add("/");
                    }
                } else {
                    for (int j = 0; j < size; j++) {
                        if (j == 0 || j == size - 1) Map[i].Add("/");
                        else Map[i].Add(" ");
                    }
                }
            }

            numeroStanzeMassimo = (6 * iSize) / 30;
            if (iSize < 30) minMonsters = 0;
            maxMonsters = iSize / 15;

            minLevel = iSize / 30;
            if (minLevel <= 0) minLevel = 1;
            maxLevel = iSize / 10;
            if (maxLevel > 9) maxLevel = 9;

            minLoot = iSize / 30;
            if (minLoot <= 0) minLoot = 1;

            maxLoot = iSize / 15;
            if (maxLoot >= tLvls.Count) maxLoot = tLvls.Count - 1;

            GenerateRooms();
            // Spawno il giocatore
            Point p = new Point(-1, -1);
            Point s = new Point(-1, -1);
            if (Rooms.Count > 0) {
                int iRndIndex = Global.rnd.Next(0, Rooms.Count);
                Room roo = Rooms[iRndIndex];
                roo.bPlayerSpawnRoom = true;

                List<Point> corn = GetCorners(roo);
                p = new Point(Global.rnd.Next(corn[0].X + 1, corn[1].X), Global.rnd.Next(corn[0].Y + 1, corn[2].Y));
            } else p = new Point(Global.rnd.Next(0, size), Global.rnd.Next(0, size));
            Map[p.Y][p.X] = "P";

            GenerateMonsters();
            GenerateTreasure();

            // Spawno le scale
            int iCounter = 0;
            if (Rooms.Count > 0) {
                do {
                    List<Room> allowedSpawnRooms = new List<Room>();
                    if (Rooms.Count > 1) allowedSpawnRooms = Rooms.Where(x => !x.bPlayerSpawnRoom).ToList();
                    else allowedSpawnRooms.AddRange(Rooms);

                    int iRndIndex = Global.rnd.Next(0, allowedSpawnRooms.Count);
                    Room roo = allowedSpawnRooms[iRndIndex];

                    List<Point> corn = GetCorners(roo);
                    s = new Point(Global.rnd.Next(corn[0].X + 1, corn[1].X), Global.rnd.Next(corn[0].Y + 1, corn[2].Y));
                    iCounter++;
                } while (iCounter <= 1000 && (p == s || !StairsSpawnConditions(s)));
                if (iCounter >= 1000) s = RandomSpawn(p); // Se la generazione è fallita con StairsSpawnConditions allora faccio senza.
            } else { s = RandomSpawn(p); }
            Map[s.Y][s.X] = "!";
        }

        public Point RandomSpawn(Point doNotOverlap) {
            Point s = new Point();
            do {
                if (Rooms.Count == 0) {
                    s = new Point(Global.rnd.Next(0, size), Global.rnd.Next(0, size));
                } else {
                    List<Room> allowedSpawnRooms = new List<Room>();
                    if (Rooms.Count > 1) allowedSpawnRooms = Rooms.Where(x => !x.bPlayerSpawnRoom).ToList();
                    else allowedSpawnRooms.AddRange(Rooms);

                    int iRndIndex = Global.rnd.Next(0, allowedSpawnRooms.Count);
                    Room roo = allowedSpawnRooms[iRndIndex];

                    List<Point> corn = GetCorners(roo);
                    s = new Point(Global.rnd.Next(corn[0].X + 1, corn[1].X), Global.rnd.Next(corn[0].Y + 1, corn[2].Y));
                }
            } while (doNotOverlap == s);
            return s;
        }

        public List<string> GetM() {
            List<string> ret = new List<string>();
            for (int i = 0; i < Map.Count; i++) {
                ret.Add("");
                for (int j = 0; j < Map.Count; j++) {
                    ret[i] += Map[i][j] + "\t";
                }
                if (ret[ret.Count - 1].EndsWith("\t")) ret[ret.Count - 1] = ret[ret.Count - 1].Substring(0, ret[ret.Count - 1].Length - 1);
            }
            return ret;
        }

        public bool IsWalkableArea(Point p) {
            return !(IsOutBounds(p.X, p.Y) || Map[p.Y][p.X] == "/");
        }

        // Permetto lo spawn delle scale solo se sono circondate da 3 muri (sopra, sotto, destra, sistra) o se tutto attorno non ci sono muri.
        public bool StairsSpawnConditions(Point p) {
            List<Point> pointsToCheck = new List<Point>();
            {
                Point up = new Point(p.X, p.Y-1);
                Point down = new Point(p.X, p.Y+1);
                Point left = new Point(p.X-1, p.Y);
                Point right = new Point(p.X+1, p.Y);
                pointsToCheck = new List<Point>() { up, down, left, right };
            }
            if (pointsToCheck.Count(x => !IsWalkableArea(x)) == 3) return true;

            {
                Point topLeft = new Point(p.X-1, p.Y-1);
                Point topRight = new Point(p.X+1, p.Y-1);
                Point middleLeft = new Point(p.X-1, p.Y);
                Point middleRight = new Point(p.X+1, p.Y);
                Point bottomRight = new Point(p.X+1, p.Y+1);
                Point bottomLeft = new Point(p.X-1, p.Y+1);
                pointsToCheck.AddRange(new List<Point>() { topLeft, topRight, middleLeft, middleRight, bottomLeft, bottomRight });
            }
            if (pointsToCheck.Count(x => IsWalkableArea(x)) == pointsToCheck.Count) return true;
            return false;
        }

        public void GenerateRooms() {
        RoomGeneration:
            int iCont = 0;
            rooms.Clear();

            while (true) {
                bool bPlaced = false;
                for (int j = 0; j < 10; j++) {
                    Point pR = new Point(Global.rnd.Next(0, size), Global.rnd.Next(0, size));
                    if (!CanBuildPath(pR)) continue;
                    if (TryPlacingR(pR, out Size roomSize)) {
                        List<Point> walls = new List<Point>();
                        for (int h = 0; h < roomSize.Height; h++) {
                            Point p = new Point(pR.X, pR.Y + h);
                            walls.Add(p);
                            p = new Point(pR.X + roomSize.Width - 1, pR.Y + h);
                            walls.Add(p);
                        }
                        for (int h = 0; h < roomSize.Width; h++) {
                            Point p = new Point(pR.X + h, pR.Y);
                            walls.Add(p);
                            p = new Point(pR.X + h, pR.Y + roomSize.Height - 1);
                            walls.Add(p);
                        }
                        walls = walls.Distinct().ToList();
                        Room ro = new Room(pR, walls, roomSize.Height, roomSize.Width);

                        bool bIsInside = false;
                        for (int i = 0; i < Rooms.Count; i++) { if (IsRInsideR(ro, Rooms[i])) { bIsInside = true; break; } }
                        if (bIsInside) continue;
                        for (int i = 0; i < Rooms.Count; i++) { if (IsRInsideR(Rooms[i], ro)) { bIsInside = true; break; } }
                        if (bIsInside) continue;
                        rooms.Add(ro);

                        for (int i = 0; i < ro.Walls.Count; i++) Map[ro.Walls[i].Y][ro.Walls[i].X] = "/";
                        bPlaced = true;
                        break;
                    }
                }
                if (!bPlaced) break;
                iCont++;
                if (iCont >= numeroStanzeMassimo) break;
            }
            if (!GenerateCorridors()) goto RoomGeneration;
        }
        public bool GenerateCorridors() {
            while (true) {
                List<Room> rNL = Rooms.Where(x => x.linked.Count < 2).ToList();
                if (rNL.Count == 0) break;
                Room r = rNL[Global.rnd.Next(0, rNL.Count)];

                Room r2 = null;
                int iRndIndex = 0;
                int iCounter = 0;
                do {
                    iCounter++;
                    iRndIndex = Global.rnd.Next(0, Rooms.Count);
                    if (r.linked.Count > 0) {
                        for (int k = 0; k < r.linked.Count; k++) {
                            if (r.linked[k].StartPoint == Rooms[iRndIndex].StartPoint) continue;
                        }
                    }
                } while (Rooms[iRndIndex].StartPoint == r.StartPoint && iCounter < 1000);
                if (iCounter >= 1000) return false;

                r2 = Rooms[iRndIndex];

                List<Point> path;
                if (!LinkR(r, r2, out path)) break;
                Corridor corr = new Corridor();
                corr.points = path;
                corr.linkedRooms.Add(r);
                corr.linkedRooms.Add(r2);

                for (int k = 0; k < path.Count; k++) {
                    Point p = path[k];
                    if (!IsOutBounds(p.X, p.Y)) {
                        if (k != path.Count - 2 && k != path.Count - 1) Map[p.Y][p.X] = "C";
                        else Map[p.Y][p.X] = "E";
                    }
                }

                r.linked.Add(r2);
            }

            for (int i = 0; i < size; i++) {
                for (int j = 0; j < size; j++) {
                    if (Map[j][i] == "C") {
                        if (!IsOutBounds(j, i - 1)) { if (Map[j][i - 1] == " ") Map[j][i - 1] = "/"; }
                        if (!IsOutBounds(j, i + 1)) { if (Map[j][i + 1] == " ") Map[j][i + 1] = "/"; }
                        if (!IsOutBounds(j - 1, i)) { if (Map[j - 1][i] == " ") Map[j - 1][i] = "/"; }
                        if (!IsOutBounds(j + 1, i)) { if (Map[j + 1][i] == " ") Map[j + 1][i] = "/"; }
                    }
                }
            }
            for (int i = 0; i < size; i++) {
                for (int j = 0; j < size; j++) {
                    if (Map[j][i] == "C" || Map[j][i] == "E") { Map[j][i] = " "; }
                }
            }
            return true;
        }
        public void GenerateMonsters() {
            List<Room> allowedSpawnRooms = new List<Room>();
            if (Rooms.Count > 1) allowedSpawnRooms = Rooms.Where(x => !x.bPlayerSpawnRoom).ToList();
            else allowedSpawnRooms.AddRange(Rooms);

            for (int i = 0; i < allowedSpawnRooms.Count; i++) {
                List<Point> corn = GetCorners(allowedSpawnRooms[i]);
                int mAmount = Global.rnd.Next(minMonsters, maxMonsters + 1);

                for (int j = 0; j < mAmount; j++) {
                    int X = Global.rnd.Next(corn[0].X + 1, corn[1].X);
                    int Y = Global.rnd.Next(corn[0].Y + 1, corn[2].Y);

                    int randomLevel = Global.rnd.Next(minLevel, maxLevel + 1);
                    Map[Y][X] = randomLevel.ToString();
                }
            }
            for (int i = 0; i < Corridors.Count; i++) {
                int iRndIndex = Global.rnd.Next(0, Corridors[i].points.Count);

                int randomLevel = Global.rnd.Next(minLevel, maxLevel + 1);
                Map[Corridors[i].points[iRndIndex].Y][Corridors[i].points[iRndIndex].X] = randomLevel.ToString();
            }
        }
        public void GenerateTreasure() {
            List<Room> allowedSpawnRooms = new List<Room>();
            if (Rooms.Count > 1) allowedSpawnRooms = Rooms.Where(x => !x.bPlayerSpawnRoom).ToList();
            else allowedSpawnRooms.AddRange(Rooms);

            for (int i = 0; i < allowedSpawnRooms.Count; i++) {
                int prob = Global.rnd.Next(0, 100);
                if (prob < 100) {
                    int lvlT = Global.rnd.Next(minLoot, maxLoot + 1);
                    Room roo = allowedSpawnRooms[i];

                    List<Point> corn = GetCorners(roo);
                    Point p = new Point(Global.rnd.Next(corn[0].X + 1, corn[1].X), Global.rnd.Next(corn[0].Y + 1, corn[2].Y));
                    Map[p.Y][p.X] = tLvls[lvlT];
                }
            }
        }

        public bool TryPlacingR(Point p, out Size roomSize) {
            roomSize = new Size(-1, -1);
            int iLocalHeight = 0;
            for (int j = 0; j < maxS; j++) {
                int iLocalWidth = 0;
                if (IsOutBounds(p.X, p.Y + j)) break;
                if (!CanBuildPath(p.X, p.Y + j)) break;
                iLocalHeight++;
                for (int i = 0; i < maxS; i++) {
                    if (IsOutBounds(p.X + i, p.Y)) break;
                    if (!CanBuildPath(p.X + i, p.Y)) break;
                    iLocalWidth++;
                }
                if (iLocalWidth < roomSize.Width || roomSize.Width == -1) roomSize.Width = iLocalWidth;
            }
            roomSize.Height = iLocalHeight;
            if (roomSize.Height == -1 || roomSize.Width == -1 || roomSize.Height == 0 || roomSize.Width == 0) return false;
            if (roomSize.Height < minS || roomSize.Width < minS) return false;

            return true;
        }

        public bool IsInsideR(Point p, Room ro) {
            if (p.X > ro.StartPoint.X && p.X < ro.StartPoint.X + ro.W - 1) {
                if (p.Y > ro.StartPoint.Y && p.Y < ro.StartPoint.Y + ro.H - 1) return true;
            }
            return false;
        }

        public bool IsRInsideR(Room ro, Room ro2) {
            List<Point> points = GetCorners(ro);
            for (int i = 0; i < points.Count; i++) if (IsInsideR(points[i], ro2)) return true;
            return false;
        }

        public List<Point> GetCorners(Room ro) {
            List<Point> corn = new List<Point>();
            corn.Add(ro.StartPoint);
            corn.Add(new Point(ro.StartPoint.X + ro.W - 1, ro.StartPoint.Y));
            corn.Add(new Point(ro.StartPoint.X, ro.StartPoint.Y + ro.H - 1));
            corn.Add(new Point(ro.StartPoint.X + ro.W - 1, ro.StartPoint.Y + ro.H - 1));
            return corn;
        }


        public bool LinkR(Room toLink, Room linkTo, out List<Point> path) {
            path = new List<Point>();

            List<Point> corn1 = GetCorners(toLink);
            List<Point> corn2 = GetCorners(linkTo);

            for (int i = 0; i < toLink.Walls.Count; i++) {
                path = new List<Point>();
                Point startPoint = toLink.Walls[i];
                if (corn1.Contains(startPoint)) continue;

                Point fakeStartPoint;
                fakeStartPoint = new Point(startPoint.X + 1, startPoint.Y);
                if (IsOutBounds(fakeStartPoint.X, fakeStartPoint.Y) || IsInsideR(fakeStartPoint, toLink) || !CanBuildPath(fakeStartPoint)) {
                    fakeStartPoint = new Point(startPoint.X - 1, startPoint.Y);
                    if (IsOutBounds(fakeStartPoint.X, fakeStartPoint.Y) || IsInsideR(fakeStartPoint, toLink) || !CanBuildPath(fakeStartPoint)) {
                        fakeStartPoint = new Point(startPoint.X, startPoint.Y + 1);
                        if (IsOutBounds(fakeStartPoint.X, fakeStartPoint.Y) || IsInsideR(fakeStartPoint, toLink) || !CanBuildPath(fakeStartPoint)) fakeStartPoint = new Point(startPoint.X, startPoint.Y - 1);
                    }
                }

                if (IsOutBounds(fakeStartPoint.X, fakeStartPoint.Y) || IsInsideR(fakeStartPoint, toLink) || !CanBuildPath(fakeStartPoint)) continue;


                for (int j = 0; j < linkTo.Walls.Count; j++) {
                    Point endPoint = linkTo.Walls[j];
                    if (corn2.Contains(endPoint)) continue;
                    Point fakeEndPoint;
                    fakeEndPoint = new Point(endPoint.X + 1, endPoint.Y);
                    if (IsOutBounds(fakeEndPoint.X, fakeEndPoint.Y) || IsInsideR(fakeEndPoint, linkTo) || !CanBuildPath(fakeEndPoint)) {
                        fakeEndPoint = new Point(endPoint.X - 1, endPoint.Y);
                        if (IsOutBounds(fakeEndPoint.X, fakeEndPoint.Y) || IsInsideR(fakeEndPoint, linkTo) || !CanBuildPath(fakeEndPoint)) {
                            fakeEndPoint = new Point(endPoint.X, endPoint.Y + 1);
                            if (IsOutBounds(fakeEndPoint.X, fakeEndPoint.Y) || IsInsideR(fakeEndPoint, linkTo) || !CanBuildPath(fakeEndPoint)) fakeEndPoint = new Point(endPoint.X, endPoint.Y - 1);
                        }
                    }

                    if (IsOutBounds(fakeEndPoint.X, fakeEndPoint.Y) || IsInsideR(fakeEndPoint, linkTo) || !CanBuildPath(fakeEndPoint)) continue;
                    if (CalcPath(path, startPoint, endPoint, fakeStartPoint, fakeEndPoint)) return true;
                }
            }
            return false;
        }

        public bool CanBuildPath(int X, int Y) { return Map[Y][X] == " " || Map[Y][X] == "C" || Map[Y][X] == "E"; }
        public bool CanBuildPath(Point p) { return CanBuildPath(p.X, p.Y); }

        private bool CalcPath(List<Point> path, Point startPoint, Point endPoint, Point fakeStartPoint, Point fakeEndPoint) {
            CalcTile start = new CalcTile();
            start.X = fakeStartPoint.X; start.Y = fakeStartPoint.Y;
            start.SetDistance(endPoint.X, endPoint.Y);

            CalcTile end = new CalcTile();
            end.X = fakeEndPoint.X; end.Y = fakeEndPoint.Y;

            List<CalcTile> activePoints = new List<CalcTile>();
            activePoints.Add(start);
            List<CalcTile> visitedPoints = new List<CalcTile>();

            while (activePoints.Any()) {
                var checkTile = activePoints.OrderBy(x => x.CD).First();

                if (checkTile.X == fakeEndPoint.X && checkTile.Y == fakeEndPoint.Y) {
                    while (checkTile.Parent != null) {
                        path.Add(new Point(checkTile.X, checkTile.Y));
                        checkTile = checkTile.Parent;
                    }
                    path.Add(new Point(checkTile.X, checkTile.Y));
                    path.Add(startPoint);
                    path.Add(endPoint);
                    return true;
                }
                visitedPoints.Add(checkTile);
                activePoints.Remove(checkTile);

                var walkableTiles = GetWalkablePoints(checkTile, end);
                foreach (var wTile in walkableTiles) {
                    if (visitedPoints.Any(x => x.X == wTile.X && x.Y == wTile.Y)) continue;
                    if (activePoints.Any(x => x.X == wTile.X && x.Y == wTile.Y)) {
                        var eTile = activePoints.First(x => x.X == wTile.X && x.Y == wTile.Y);
                        if (eTile.CD > checkTile.CD) {
                            activePoints.Remove(eTile);
                            activePoints.Add(wTile);
                        }
                    } else activePoints.Add(wTile);
                }
            }
            return false;
        }

        public List<CalcTile> GetWalkablePoints(CalcTile currentTile, CalcTile target) {
            List<CalcTile> possibleTiles = new List<CalcTile>() {
        new CalcTile { X= currentTile.X, Y= currentTile.Y-1, Parent = currentTile, C = currentTile.C+1},
        new CalcTile { X= currentTile.X, Y= currentTile.Y+1, Parent = currentTile, C = currentTile.C+1},
        new CalcTile { X= currentTile.X+1, Y= currentTile.Y, Parent = currentTile, C = currentTile.C+1},
        new CalcTile { X= currentTile.X-1, Y= currentTile.Y, Parent = currentTile, C = currentTile.C+1}
      };
            possibleTiles.ForEach(x => x.SetDistance(target.X, target.Y));

            return possibleTiles.Where(x => !IsOutBounds(x.X, x.Y)).Where(x => CanBuildPath(x.X, x.Y)).ToList();
        }
    }

    class CalcTile {
        public int X = 0, Y = 0;
        public int C = 0;
        public int D = 0;
        public int CD => C + D;
        public CalcTile Parent = null;

        public void SetDistance(int targetX, int targetY) {
            D = Math.Abs(X - targetX) + Math.Abs(Y - targetY);
        }
    }

    class Room {
        private Point startPoint;
        public Point StartPoint { get { return startPoint; } }

        private int h, w;
        public int H { get { return h; } }
        public int W { get { return w; } }

        private List<Point> walls = new List<Point>();
        public List<Point> Walls { get { return walls; } }

        public List<Room> linked = new List<Room>();

        public bool bPlayerSpawnRoom = false;

        public Room(Point startPoint, List<Point> walls, int h, int w) {
            this.startPoint = startPoint;
            this.walls = walls;
            this.h = h;
            this.w = w;
        }
    }

    class Corridor {
        public List<Point> points = new List<Point>();
        public List<Room> linkedRooms = new List<Room>();
    }
}
