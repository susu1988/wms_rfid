﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.Wms.DbModel;

namespace THOK.Wms.Bll.Interfaces
{
    public interface ICellService : IService<Cell>
    {
        object GetDetails(int page, int rows, string cellCode);

        bool Add(Cell cell);

        bool Delete(string cellCode);

        bool Save(Cell cell);

        object GetWareCheck(string shelfCode);

        object GetSearch(string wareCode);

        object FindCell(string parameter);

        object GetCell(string shelfCode);

        
    }
}