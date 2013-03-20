using System;
using CMS.CMSHelper;
using CMS.SiteProvider;

namespace PullKentico
{
   class Program
    {
        static void Main(string[] args)
        {
            DataPull dataPull = new DataPull();
            CMSContext.Init();
            
            SiteInfo site = SiteInfoProvider.GetSiteInfo("ClientSiteName");
            SiteInfoProvider.CurrentSiteID = site.SiteID;
            SiteInfoProvider.CurrentSiteName = site.SiteName;
            
            // You should only leave ONE of these uncommented! -- Keith Murphy / Murfbard.com 3/20/2013 (KM1) 
            //dataPull.AutomaticRun();
            dataPull.ManualRun();
        }


    }
}