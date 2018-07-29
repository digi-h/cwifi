/* 
 * C# managed version of Native Wi-Fi API present on Windows.
 * http://managedwifi.codeplex.com/ .
 *
 * Licence: MIT License(MIT) Copyright(c) 2018 K. Lean
 * 
 */

using NativeWifi;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace cwifi
{

    public partial class frmCwifi : Form
    {
        /// <summary>
        /// Converts a 802.11 SSID to a string.
        /// </summary>
        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        public frmCwifi()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            RefreshNetworks();
        }

        private void RefreshNetworks()
        {
            comboBox1.Items.Clear();
            listView1.Items.Clear();

            using (WlanClient client = new WlanClient())
            {
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    comboBox1.Items.Add(wlanIface.InterfaceName + " (" + wlanIface.InterfaceGuid + ")");

                    Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
                    Wlan.WlanBssEntry[] bssLists = wlanIface.GetNetworkBssList();

                    foreach (Wlan.WlanAvailableNetwork n in networks)
                    {
                        string ssid = GetStringForSSID(n.dot11Ssid);
                        int rssi = -1;
                        int channelMHz = -1;
                        int channel = -1;
                        string mac_address = "?";

                        // Isolation of the network is not detected via this method?
                        //Wlan.WlanBssEntry[] bssLists = wlanIface.GetNetworkBssList(network.dot11Ssid, Wlan.Dot11BssType.Any, network.securityEnabled);

                        foreach (var b in bssLists)
                        {
                            if (b.dot11Ssid.SSID[0] == n.dot11Ssid.SSID[0])
                            {
                                rssi = b.rssi;
                                channelMHz = (int)b.chCenterFrequency / 1000; // in KHz --> converted to MHz

                                // format the channel frequency - https://en.wikipedia.org/wiki/List_of_WLAN_channels

                                if (channelMHz > 0)
                                {
                                    switch (channelMHz) // These are only the 2.4 GHz Channes TODO: Add 5 GHz Channels
                                    {
                                        case 2412: // b/g/n
                                            channel = 1;
                                            break;
                                        case 2417: // non-standard
                                            channel = 2;
                                            break;
                                        case 2422: // n
                                            channel = 3;
                                            break;
                                        case 2427: // non-standard
                                            channel = 4;
                                            break;
                                        case 2432: // g/n
                                            channel = 5;
                                            break;
                                        case 2437: // b
                                            channel = 6;
                                            break;
                                        case 2442: // non-standard
                                            channel = 7;
                                            break;
                                        case 2447: // non-standard
                                            channel = 8;
                                            break;
                                        case 2452: // g/n
                                            channel = 9;
                                            break;
                                        case 2457: // non-standard
                                            channel = 10;
                                            break;
                                        case 2462: // b/n
                                            channel = 11;
                                            break;
                                        case 2467: // non-standard
                                            channel = 12;
                                            break;
                                        case 2472: // g/n
                                            channel = 13;
                                            break;
                                        case 2484: // b
                                            channel = 14;
                                            break;
                                    }

                                }

                                PhysicalAddress pa = new PhysicalAddress(n.dot11Ssid.SSID);
                                mac_address = pa.ToString();

                                // format the mac address:
                                for (int i = 0; i < mac_address.Length - 1; i++)
                                {
                                    if (i % 3 == 0)
                                    {
                                        mac_address = mac_address.Insert(i, "-");
                                    }
                                }

                                mac_address = mac_address.Remove(0, 1);
                            }
                        }

                        List<char> phy_types_supported = new List<char>();

                        foreach (var p in n.Dot11PhyTypes)
                        {
                            switch ((int)p)
                            {
                                case 4: // Wlan.Dot11PhyType.OFDM:
                                    phy_types_supported.Add('a'); // 4
                                    break;
                                case 6: // Wlan.Dot11PhyType.ERP:
                                    phy_types_supported.Add('g'); // 6
                                    break;
                                case 7:
                                    phy_types_supported.Add('n'); // 7
                                    break;
                            }
                        }

                        string phy = "?";

                        if (phy_types_supported.Count > 0)
                        {
                            phy = "802.11";

                            foreach (var p in phy_types_supported)
                            {
                                phy += p;
                            }
                        }

                        // https://www.howtogeek.com/197268/how-to-find-the-best-wi-fi-channel-for-your-router-on-any-operating-system/
                        string[] itemArray = { ssid, channel + " (" + channelMHz + " MHz)", mac_address, phy, rssi.ToString(), n.wlanSignalQuality.ToString() };
                        listView1.Items.Add(new ListViewItem(itemArray));

                        /*
                        if (network.dot11DefaultCipherAlgorithm == Wlan.Dot11CipherAlgorithm.WEP)
                        {
                            Console.WriteLine("Found WEP network with SSID {0}.", GetStringForSSID(network.dot11Ssid));
                        }*/
                    }

                    toolStripStatusLabel1.Text = string.Format("{0} network(s) detected.", listView1.Items.Count);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshNetworks();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
