using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;

namespace SpaceWeatherAPI.CustomQueryParameters
{
    public class QueryParameters
    {
        /// <summary>
        /// Sets the search term for filtering results.
        /// **Filter On Columns:** Name, WeatherInfo.Condition
        /// </summary>
        /// <remarks>For example: "Gezegen" or "Blizzard"</remarks>
        public string? SearchTerm { get; set; } = null;

        private string? _column = "id";

        private string? _order = "asc";

        /// <summary>
        /// Sets the sorting column and order.
        /// **Example:** "name_asc"
        /// </summary>
        /// <remarks>For example: "name_asc"</remarks>
        public string Sort
        {
            get
            {
                return $"{_column}_{_order}";
            }
            set
            {
                var parameter = value.Split('_');
                _column = parameter[0];
                if(parameter.Length > 1)
                {
                    _order = parameter[1];
                }
            }
        }

    
        public string GetColumn()
        {
            return _column;
        }

  
        public string GetOrder()
        {
            return _order;
        }

        /// <summary>
        /// Sets the page number.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Sets the page size.
        /// </summary>
        public int PageSize { get; set; } = 5;

        public void ValidationPageParams()
        {
            this.PageNumber = PageNumber < 1 ? 1 : PageNumber;
            this.PageSize = PageSize < 1 ? 5 : PageSize;
        }

    }
}
