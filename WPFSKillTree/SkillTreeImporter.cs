using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;

namespace POESKillTree
{
    public static class SkillTreeImporter
    {
        /// <summary>
        /// Loads from the unofficial online tool
        /// </summary>
        public static void LoadBuildFromPoezone( SkillTree tree, string buildUrl )
        {
            if ( !buildUrl.Contains( '#' ) ) throw new FormatException( );

            const string dataUrl = "http://poezone.ru/skilltree/data.js";
            const string buildPostUrl = "http://poezone.ru/skilltree/";
            string build = buildUrl.Substring( buildUrl.LastIndexOf( '#' ) + 1 );

            string dataFile, buildFile;
            {
                HttpWebRequest req = ( HttpWebRequest )WebRequest.Create( dataUrl );
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534.30 (KHTML, like Gecko) Iron/12.0.750.0 Chrome/12.0.750.0 Safari/534.30";
                WebResponse resp = req.GetResponse( );
                dataFile = new StreamReader( resp.GetResponseStream( ) ).ReadToEnd( );
            }

            {
                string postData = "build=" + build;
                byte[] postBytes = Encoding.ASCII.GetBytes( postData );
                HttpWebRequest req = ( HttpWebRequest )WebRequest.Create( buildPostUrl );
                req.Method = "POST";
                req.ContentLength = postBytes.Length;
                req.ContentType = "application/x-www-form-urlencoded";
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534.30 (KHTML, like Gecko) Iron/12.0.750.0 Chrome/12.0.750.0 Safari/534.30";
                req.Accept = "application/json, text/javascript, */*; q=0.01";
                req.Host = "poezone.ru";
                req.Referer = "http://poezone.ru/skilltree/";
                req.AutomaticDecompression = DecompressionMethods.GZip;
                //req.Headers.Add( "Accept", "application/json, text/javascript" );
                req.Headers.Add( "Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.3" );
                req.Headers.Add( "Accept-Encoding", "gzip,deflate,sdch" );
                req.Headers.Add( "Accept-Language", "en-US,en;q=0.8" );

                //req.Headers.Add( "Connection", "keep-alive" );
                //req.Headers.Add( "Host", "poezone.ru" );
                req.Headers.Add( "Origin", "http://poezone.ru" );
                //req.Headers.Add( "Referer", "http://poezone.ru/skilltree/" );
                //req.Headers.Add( "User-Agent", );
                req.Headers.Add( "X-Requested-With", "XMLHttpRequest" );
                req.Expect = "";
                req.Credentials = CredentialCache.DefaultCredentials;

                Stream dataStream = req.GetRequestStream( );
                dataStream.Write( postBytes, 0, postBytes.Length );
                dataStream.Close( );

                WebResponse resp = req.GetResponse( );
                var status = ( resp as HttpWebResponse ).StatusDescription;
                buildFile = new StreamReader( resp.GetResponseStream( ) ).ReadToEnd( );
            }

            if ( !buildFile.Contains( "[" ) )
            {
                MessageBox.Show( "Build does not exist and/or is corrupt!" );
                return;
            }

            // position decompose
            List<Vector2D?> positions = new List<Vector2D?>( );
            var lines = dataFile.Split( '\n' );
            foreach ( var line in lines )
                if ( line.StartsWith( "skillpos=" ) )
                {
                    string posString = line.Substring( line.IndexOf( '[' ) + 1, line.LastIndexOf( ']' ) - line.IndexOf( '[' ) - 1 );
                    StringBuilder sb = new StringBuilder( );
                    bool inBracket = false;
                    foreach ( var c in posString )
                    {
                        if ( !inBracket && c == ',' )
                        {
                            positions.Add( sb.Length == 0 ? null : new Vector2D?( new Vector2D(
                                    int.Parse( sb.ToString( ).Split( ',' )[ 0 ] ),
                                    int.Parse( sb.ToString( ).Split( ',' )[ 1 ] )
                                ) ) );
                            sb.Clear( );
                        }
                        else
                        {
                            if ( c == '[' ) inBracket = true;
                            else if ( c == ']' ) inBracket = false;
                            else sb.Append( c );
                        }
                    }
                    positions.Add( sb.Length == 0 ? null : new Vector2D?( new Vector2D(
                            int.Parse( sb.ToString( ).Split( ',' )[ 0 ] ),
                            int.Parse( sb.ToString( ).Split( ',' )[ 1 ] )
                        ) ) );
                }

            // min max
            double minx = float.MaxValue, miny = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue;
            foreach ( var posn in positions )
            {
                if ( !posn.HasValue ) continue;
                var pos = posn.Value;
                minx = Math.Min( pos.X, minx );
                miny = Math.Min( pos.Y, miny );
                maxx = Math.Max( pos.X, maxx );
                maxy = Math.Max( pos.Y, maxy );
            }

            double nminx = float.MaxValue, nminy = float.MaxValue, nmaxx = float.MinValue, nmaxy = float.MinValue;
            foreach ( var node in tree.Skillnodes.Values )
            {
                var pos = node.Position;
                nminx = Math.Min( pos.X, nminx );
                nminy = Math.Min( pos.Y, nminy );
                nmaxx = Math.Max( pos.X, nmaxx );
                nmaxy = Math.Max( pos.Y, nmaxy );
            }

            //respose
            var buildResp = buildFile.Replace( "[", "" ).Replace( "]", "" ).Split( ',' );
            int character = int.Parse( buildResp[ 0 ] );
            List<int> skilled = new List<int>( );

            tree.Chartype = character;
            tree.SkilledNodes.Clear( );
            SkillTree.SkillNode startnode = tree.Skillnodes.First( nd => nd.Value.name == tree.CharName[ tree.Chartype ].ToUpper( ) ).Value;
            tree.SkilledNodes.Add( startnode.id );

            for ( int i = 1 ; i < buildResp.Length ; ++i )
            {
                if ( !positions[ int.Parse( buildResp[ i ] ) ].HasValue ) Debugger.Break( );

                var poezonePos = ( positions[ int.Parse( buildResp[ i ] ) ].Value - new Vector2D( minx, miny ) ) * new Vector2D( 1 / ( maxx - minx ), 1 / ( maxy - miny ) );
                double minDis = 2;
                KeyValuePair<ushort, SkillTree.SkillNode> minNode = new KeyValuePair<ushort, SkillTree.SkillNode>();
                foreach ( var node in tree.Skillnodes )
                {
                    var nodePos = ( node.Value.Position - new Vector2D( nminx, nminy ) ) * new Vector2D( 1 / ( nmaxx - nminx ), 1 / ( nmaxy - nminy ) );
                    double dis = ( nodePos - poezonePos ).Length;
                    if ( dis < minDis )
                    {
                        minDis = dis;
                        minNode = node;
                    }
                }

                tree.SkilledNodes.Add( minNode.Key );
            }
            tree.UpdateAvailNodes( );

            //string dataFile = 
        }
    }
}
