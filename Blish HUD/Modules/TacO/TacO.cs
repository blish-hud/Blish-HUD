using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Entities.Paths;
using Blish_HUD.Modules.TacO.Origin;
using Blish_HUD.Modules.PoiLookup;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.TacO {
    public class TacO : Module {

        public override ModuleInfo GetModuleInfo() {
            return new ModuleInfo(
              "(Compatibility) TacO",
              "bh.compatibility.taco",
              "Brings compatibility for TacO marker and path files as well as several other TacO features.",
              "BoyC (Ported by LandersXanders.1235)",
              "1"
             );
        }

        public override void DefineSettings(Settings settings) {
            
        }

        // ------ //

        // This content is ported directly from the TacO source code *with permission*.

        public Dictionary<string, GW2TacticalCategory> CategoryMap = new Dictionary<string, GW2TacticalCategory>();

        public Dictionary<Guid, POI> POIs = new Dictionary<Guid, POI>();
        public List<POIRoute> Routes = new List<POIRoute>();
        public Dictionary<Guid, GW2Trail> Trails = new Dictionary<Guid, GW2Trail>();
        public List<POI> mapPOIs;

        public void FindClosestRouteMarkers(bool force) {
            for (int x = 0; x < Routes.Count; x++) {
                if (!force && Routes[x].ActiveItem != -1) continue;

                if (Routes[x].MapId == GameService.Player.MapId && Routes[x].HasResetPos && (Routes[x].ResetPos - GameService.Player.Position).Length() < Routes[x].ResetRad)
                    Routes[x].ActiveItem = 0;

                float closestDist = 1000000000;
                // Never actually used
                //int closest = -1;

                for (int y = 0; y < Routes[x].Route.Length; y++) {
                    var g = Routes[x].Route[y];

                    if (POIs.ContainsKey(g)) {
                        var p = POIs[g];
                        if (p.MapId != GameService.Player.MapId)
                            continue;

                        float dist = (p.Position - GameService.Player.Position).Length();
                        if (dist < closestDist) {
                            closestDist = dist;
                            // Never actually used
                            //closest = y;
                            Routes[x].ActiveItem = y;
                        }
                    }
                }
            }
        }

        public GW2TacticalCategory GetCategory(string s) {
            s = s.ToLower();
            if (CategoryMap.ContainsKey(s))
                return CategoryMap[s];

            return null;
        }

        public int DictionaryHash(Guid i) {
            // This should hopefully be identical to BoyC's implementation
            return i.GetHashCode();
        }

        public int DictionaryHash(POIActivationDataKey i) {
            // This should hopefully be identical to BoyC's implementation
            return i.GetHashCode();
        }
        
        public float DistPointPlane(Vector3 vPoint, Plane plane) {
            // Need to evaluate since Vector3 can't be added to a float
            throw new NotImplementedException();
            //return (vPoint * plane.Normal) + plane.D;
        }

        public float DistRayPlane(Vector3 vRayOrigin, Vector3 vnRayVector, Vector3 vnPlaneNormal, float planeD) {
            // Need to evaluate since Vector3 can't be converted to a float (is it wanting magnitude?)
            throw new NotImplementedException();
            //float cosAlpha;
            //float deltaD;

            //cosAlpha = (vnRayVector * vnPlaneNormal);
        }

        public bool TestFrustrum(Vector3 c, Plane[] planes, int skip) {
            if (planes.Length != 4)
                throw new ArgumentException("planes should be Plane[4]");

            bool v = c.Z > 0;

            for (int x = 0; x < 4; x++)
                if (x != skip)
                    v = v && (DistPointPlane(c, planes[x]) < 0);

            return v;
        }

        public void SetRotate(ref Matrix m, float x, float y, float z, float phi) {
            // This will likely lose all other positional data stored in the matrix
            // Need to evaluate if these 3D math functions can just be handled by the Path & Marker entities
            m = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(new Vector3(x, y, z), phi));
        }

        public void InsertPOI(POI poi) {
            throw new NotImplementedException();

            if (poi.MapId != GameService.Player.MapId)
                return;

            if (poi.RouteMember) {
                bool discard = true;

                for (int y = 0; y < Routes.Count; y++) {
                    if (Routes[y].ActiveItem >= 0) {
                        if (Routes[y].Route[Routes[y].ActiveItem] == poi.Guid) {
                            discard = false;
                            break;
                        }
                    }
                }

                if (discard)
                    return;
            }

            // Convert our camera to TacO camera - need to confirm this is correct
            var cam = GameService.Camera.View;
            //poi.CameraSpacePosition = new Vector4(poi.Position.X, poi.Position.Y + poi.TypeData.Height, poi.Position.Z, 1.0f) * cam;

            if (poi.TypeData.FadeFar >= 0 && poi.TypeData.FadeNear >= 0) {
                float dist = WorldToGameCoords(poi.CameraSpacePosition.Length());
                if (dist > poi.TypeData.FadeFar)
                    return;
            }

            mapPOIs.Add(poi);
        }

        public bool FindSavedCategory(GW2TacticalCategory t) {
            if (t.KeepSaveState) return true;

            for (int x = 0; x < t.Children.Count; x++)
                if (FindSavedCategory(t.Children[x])) return true;

            return false;
        }

        public void ExportPOI(XmlNode n, POI p) {
            if (n.OwnerDocument == null)
                throw new ArgumentException("XmlNode n must have OwnerDocument defined");

            var t = n.OwnerDocument.CreateElement("POI");
            t.SetAttribute("MapID", p.MapId.ToString());
            t.SetAttribute("xpos", p.Position.X.ToString());
            t.SetAttribute("ypos", p.Position.Y.ToString());
            t.SetAttribute("zpos", p.Position.Z.ToString());

            if (!string.IsNullOrWhiteSpace(p.Name.Trim()))
                t.SetAttribute("text", p.Name);

            if (!string.IsNullOrWhiteSpace(p.Type.Trim()))
                t.SetAttribute("type", p.Type);

            t.SetAttribute("GUID", Origin.Util.GuidToBase64(p.Guid));

            p.TypeData.Write(t);

            n.AppendChild(t);
        }

        public void ExportTrail(XmlNode n, GW2Trail p) {
            if (n.OwnerDocument == null)
                throw new ArgumentException("XmlNode n must have OwnerDocument defined");

            var t = n.OwnerDocument.CreateElement("Trail");
            if (!string.IsNullOrWhiteSpace(p.Type.Trim()))
                t.SetAttribute("type", p.Type);

            t.SetAttribute("Guid", Origin.Util.GuidToBase64(p.Guid));
            p.TypeData.Write(t);

            n.AppendChild(t);
        }

        public float WorldToGameCoords(float world) {
            return world / 0.0254f;
        }

        public float GameToWorldCoords(float game) {
            return game * 0.0254f;
        }

    }
}
