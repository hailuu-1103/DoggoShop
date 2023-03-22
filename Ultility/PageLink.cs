using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Drawing;

namespace WebRazor.Materials
{
    public class PageLink
    {
        int perPage;
        public PageLink(int perPage)
        {
            this.perPage = perPage;
        }

        private int CalcPagesCount(int size)
        {
            int totalPage = size / perPage;

            if (size % perPage != 0) totalPage++;
            return totalPage;
        }

        public List<String> getLink(int Page, int size, string baseUrl)
        {
            List<String> PagesLink = new List<string>();

            int total = CalcPagesCount(size);

            if (Page < 1 || Page > total)
            {
                return PagesLink;
            }

            if (Page != 1)
            {
                String link = "<a href=\"" + baseUrl + "page=" + (Page - 1) + "\">&laquo;</a>";
                PagesLink.Add(link);
            }

            int startPage = 1;
            int endPage = total;
            if (total >= 10)
            {
                if (total - Page <= 5)
                {
                    endPage = total;
                    startPage = total - 9;
                }
                else
                {
                    if (Page - 5  < 1)
                    {
                        endPage = 10;
                        startPage = 1;
                    } else
                    {
                        startPage = Page - 5;
                        endPage = startPage + 9;
                    }
                }

            }

            for (int i = startPage; i <= endPage; i++)
            {
                String content = "";
                if (i == Page)
                {
                    content = "class=\"active\"";
                }
                String link = "<a " + content + " href=\"" + baseUrl + "page=" + i + "\">" + i + "</a>";
                PagesLink.Add(link);
            }

            if (Page != total)
            {
                String link = "<a href=\"" + baseUrl + "page=" + (Page + 1) + "\">&raquo;</a>";
                PagesLink.Add(link);
            }

            return PagesLink;

        }
    }
}
