using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PathFinder
{
    class TMap
    {
        enum CellType  //типы ячеек сетки
        {
            EMPTY,
            WALL,
        }
        public enum EditMode  //режимы редактирования
        {
            SET_START,
            SET_END,
            SET_WALL,
            CLEAR
        }
        struct TCell
        {
            public int G; //пройденный путь
            public CellType type;//тип ячейки (пустота или стена)
            public Point parent; //координаты родительской ячейки(из которой мы пришли в эту)
            public void Set(int use_G, Point use_parent)
            {
                parent = use_parent;
                G = use_G;
            }
            public TCell(CellType use_type)
            {
                parent = new Point(-1, -1);
                G = 0;
                type = use_type;
            }
        }

        TCell[,] map; //карта
        Point[] path; //найденный маршрут
        int path_high;//длина найденного маршрута (если не найден то -1)

        Point start, end; //координаты начальной и конечной точек

        int map_size; //размер карты в ячейка
        int brush_size; //размер кисти редактирования в ячейках
        int cell_size; //размер ячейки в пикселах

        EditMode curr_mode; //текущий режим редактирования

        int path_length, path_cells_count;

        Graphics canvas; //объект отрисовки
        Graphics output_control; //объект на который выводим изображение
        Bitmap image; //промежуточный буфер в который рисуем

        Point selected_cell; //координаты ячейки под курсором

        Point[] open_list, closed_list; //массивы открытых и закрытых ячеек (использ. при нахожд. маршрута)
        int open_list_high, closed_list_high;

        public TMap(Graphics use_output_control, int use_canvas_size, int use_map_size)
        {
            map_size = use_map_size;
            cell_size = use_canvas_size / use_map_size;

            output_control = use_output_control;
            brush_size = 1;
            image = new Bitmap(use_canvas_size, use_canvas_size);
            canvas = Graphics.FromImage(image);

            map = new TCell[map_size, map_size];
            path = new Point[map_size * map_size];

            selected_cell = new Point(0, 0);
            curr_mode = EditMode.SET_START;

            //генерируем случайные препятствия
            Random rand = new Random();
            for (int i = 0; i < map_size; i++)
                for (int k = 0; k < map_size; k++)
                    map[i, k].type = rand.Next(10) < 2 ? CellType.WALL : CellType.EMPTY;

            start = new Point(0, 0);
            end = new Point(2, 2);
            //чтобы в начальной и конечной ячейки не содержалось препятствий
            SetCell(start, new TCell(CellType.EMPTY));
            SetCell(end, new TCell(CellType.EMPTY));

            //массивы используемые в поиске пути
            open_list = new Point[map_size * map_size];
            open_list_high = -1;
            closed_list = new Point[map_size * map_size];
            closed_list_high = -1;
        }
        int FindInList(Point[] list, int high, Point val)
        {
            for (int i = 0; i <= high; i++)
                if (list[i] == val) return i;
            return -1;
        }
        void RemoveFromList(Point[] list, ref int high, int val_id)
        {
            list[val_id] = list[high--];
        }
        TCell GetCell(Point cell)
        {
            return map[cell.X, cell.Y];
        }
        void SetCell(Point cell, TCell val)
        {
            map[cell.X, cell.Y] = val;
        }
        Rectangle GetCellRect(Point cell)
        {
            return new Rectangle(
                cell.X * cell_size,
                cell.Y * cell_size,
                cell_size, cell_size);
        }
        public void Draw()
        {
            Pen red_pen = new Pen(Color.Red, 1);
            //заливаем все черным цветом
            canvas.Clear(Color.Black);
            //Выделяем начальную ячейка
            canvas.FillRectangle(Brushes.Green, GetCellRect(start));
            //Выделяем конечную ячейку
            canvas.FillRectangle(Brushes.Blue, GetCellRect(end));
            //отрисовываем препятствия
            for (int i = 0; i < map_size; i++)
                for (int k = 0; k < map_size; k++)
                    if (map[i, k].type != CellType.EMPTY)
                        canvas.FillRectangle(Brushes.Brown, GetCellRect(new Point(i, k)));
            //отрисовываем найденный маршрут
            for (int i = 0; i <= path_high; i++)
                canvas.FillRectangle(Brushes.Olive, GetCellRect(path[i]));
            //Выделяем текущую выбранную ячейку(ячейки)
            if (curr_mode == EditMode.SET_START || curr_mode == EditMode.SET_END)
                canvas.DrawRectangle(red_pen, GetCellRect(selected_cell));
            else
            {
                int sx = selected_cell.X;
                int sy = selected_cell.Y;
                for (int i = -brush_size + 1; i <= brush_size - 1; i++)
                    for (int k = -brush_size + 1; k <= brush_size - 1; k++)
                        if (sx + i >= 0 && sx + i < map_size && sy + k >= 0 && sy + k < map_size)
                            canvas.DrawRectangle(red_pen, GetCellRect(new Point(sx + i, sy + k)));
            }
            //выводим отрисованное изображение на форму
            output_control.DrawImage(image, 0, 0);
        }
        public void OnMouseMove(Point use_mouse_pos)
        {
            selected_cell = new Point(use_mouse_pos.X / cell_size, use_mouse_pos.Y / cell_size);
            //проверяем выход за границы карты
            if (selected_cell.X >= map_size) selected_cell.X = map_size - 1;
            if (selected_cell.Y >= map_size) selected_cell.Y = map_size - 1;
            if (selected_cell.X < 0) selected_cell.X = 0;
            if (selected_cell.Y < 0) selected_cell.Y = 0;
        }
        public void SetEditMode(EditMode use_mode)
        {
            curr_mode = use_mode;
        }
        public EditMode GetEditMode()
        {
            return curr_mode;
        }
        public void SetBrushSize(int use_brush_size)
        {
            brush_size = use_brush_size;
        }
        public int GetPathLength()
        {
            return path_length;
        }
        public int GetPathCellsCount()
        {
            return path_cells_count;
        }
        bool EditCell(Point cell)
        {
            //реакция на нажатие клавиши мыши
            //в зависимости от текущего режима редактирования
            switch (curr_mode)
            {
                case EditMode.SET_START:
                    if (start != cell && GetCell(cell).type == CellType.EMPTY && cell != end)
                    {
                        start = cell;
                        return true;
                    }
                    break;
                case EditMode.SET_END:
                    if (end != cell && GetCell(cell).type == CellType.EMPTY && cell != start)
                    {
                        end = cell;
                        return true;
                    }
                    break;
                case EditMode.SET_WALL:
                    if (GetCell(cell).type != CellType.WALL && cell != start && cell != end)
                    {
                        SetCell(cell, new TCell(CellType.WALL));
                        return true;
                    }
                    break;
                case EditMode.CLEAR:
                    if (GetCell(cell).type != CellType.EMPTY)
                    {
                        SetCell(cell, new TCell(CellType.EMPTY));
                        return true;
                    }
                    break;
            }
            return false;
        }

        public void OnMouseDown()
        {
            bool need_update = false;
            if (curr_mode == EditMode.SET_START || curr_mode == EditMode.SET_END)
            {
                need_update = EditCell(selected_cell);
            }
            else
            {
                int sx = selected_cell.X;
                int sy = selected_cell.Y;
                for (int i = -brush_size + 1; i <= brush_size - 1; i++)
                    for (int k = -brush_size + 1; k <= brush_size - 1; k++)
                        if (sx + i >= 0 && sx + i < map_size && sy + k >= 0 && sy + k < map_size)
                        {
                            need_update = need_update | EditCell(new Point(sx + i, sy + k));
                        }
            }
            //если что-то изменилось то ищем путь по новой
            if (need_update)
                FindPath();
        }

        //находит ячейку с минимальным пройденным путем
        int FindMinCost(Point[] list, int high)
        {
            System.Diagnostics.Debug.Assert(high >= 0);
            int curr_min = 0;
            for (int i = 1; i <= high; i++)
                if (GetCell(list[i]).G < GetCell(list[curr_min]).G) curr_min = i;
            return curr_min;
        }
        int GetMovementCost(int i, int k)
        {
            return i * k == 0 ? 10 : 14;
        }
        void FindPath()
        {
            open_list_high = -1;
            closed_list_high = -1;
            open_list[++open_list_high]=start;
            bool path_found = false;
            while (open_list_high >= 0)
            {
                int curr_cell = FindMinCost(open_list, open_list_high);
                Point cell = open_list[curr_cell];
                RemoveFromList(open_list, ref open_list_high, curr_cell);
                closed_list[++closed_list_high] = cell;
                if (cell == end)
                {
                    path_found = true;
                    break;
                }
                //для каждой из 8 соседних ячеек
                for (int i = cell.X > 0 ? -1 : 0; i <= (cell.X < map_size - 1 ? 1 : 0); i++)
                    for (int k = cell.Y > 0 ? -1 : 0; k <= (cell.Y < map_size - 1 ? 1 : 0); k++)
                        if (!(i == 0 && k == 0))
                        {
                            Point adj_point = new Point(cell.X + i, cell.Y + k);
                            TCell adj = GetCell(adj_point);
                            //если препятствие то пропускаем
                            if (adj.type == CellType.WALL) continue;
                            //если происходит срез препятствия по диагонали, то тоже пропускаем
                            if (i != 0 && k != 0)
                            {
                                if (map[cell.X + i, cell.Y].type == CellType.WALL) continue;
                                if (map[cell.X, cell.Y + k].type == CellType.WALL) continue;
                            }
                            //если в закрытом списке то пропускаем
                            if (FindInList(closed_list, closed_list_high, adj_point) != -1) continue;
                            //если не в отрытом списке то добавляем
                            int t = FindInList(open_list, open_list_high, adj_point);
                            if (t == -1)
                            {
                                open_list[++open_list_high] = adj_point;
                                //задаем родителя и G
                                int G = GetCell(cell).G + GetMovementCost(i, k);
                                map[cell.X + i, cell.Y + k].Set(G, cell);
                            }
                            else if (GetCell(cell).G + GetMovementCost(i, k) < adj.G)//иначе оцениваем G
                            {
                                adj.G = GetCell(cell).G + GetMovementCost(i, k);
                                map[adj_point.X, adj_point.Y].parent = cell;
                            }
                        }
            }
            //добавляем маршрут от конечной до начальной точки
            path_high = -1;
            path_length = 0;
            path_cells_count = 0;
            if (path_found)
            {
                Point curr_cell = end;
                Point parent = GetCell(curr_cell).parent;
                path_length += GetMovementCost(curr_cell.X - parent.X, curr_cell.Y - parent.Y);
                while (parent != start)
                {
                    path[++path_high] = parent;
                    curr_cell = parent;
                    parent = GetCell(curr_cell).parent;
                    path_cells_count++;
                    path_length += GetMovementCost(curr_cell.X - parent.X, curr_cell.Y - parent.Y);
                }
            }
        }
    }
}