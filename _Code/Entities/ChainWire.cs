using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    /// Lingo source, with my added comments
    /// The "copyPixels" function effectively "draws" the image in parameter one onto the parent image, which in our case is "drawing to the RenderTarget, but instead we're going to add the images with rotations and shit.
    /*dr = MoveToPoint(pos, lastPos, 1.0) -- Okay now who the fuck named this. This function is actually "dr" or, "the normalized difference of two vectors"
      dst = diag(pos, lastPos) -- Diag refers to the Distance function sqrt((x2-x1)^2 + (y2-y1)^2)
      if((num mod 2)=0)then
        wdth = 10
      else
        wdth = 3.5
      end if
      
      pntA = lastPos + dr*11 -- Since we only use dr Here, we can move the *11 to MoveToPoint construction.
      pntB = pos - dr*11
      
      pastQd = [pntA - lastPerp*wdth, pntA + lastPerp*wdth, pntB + lastPerp*wdth, pntB - lastPerp*wdth]
      pastQd = pastQd - [gRenderCameraTilePos*20, gRenderCameraTilePos*20, gRenderCameraTilePos*20, gRenderCameraTilePos*20]
      
      if(prop.nm = "Large Chain")then
        repeat with a = 0 to 5 then
          pstDp = restrict(dp + a, 0, 29)
          member("layer"&string(pstDp)).image.copyPixels(member("largeChainGraf").image, pastQd, rect(((num mod 2)=1)*20,1+a*50,20 + ((num mod 2)=1)*7,1+(a+1)*50), {#ink:36})
          member("layer"&string(pstDp)).image.copyPixels(member("largeChainGrafHighLight").image, pastQd + [point(-2,-2), point(-2,-2), point(-2,-2), point(-2,-2)], rect(((num mod 2)=1)*20,1+a*50,20 + ((num mod 2)=1)*7,1+(a+1)*50), {#ink:36})
          
          pstDp = restrict(dp + 4 + a, 0, 29)
          b = 5 - a
          member("layer"&string(pstDp)).image.copyPixels(member("largeChainGraf").image, pastQd, rect(((num mod 2)=1)*20,1+b*50,20 + ((num mod 2)=1)*7,1+(b+1)*50), {#ink:36})
          member("layer"&string(pstDp)).image.copyPixels(member("largeChainGrafHighLight").image, pastQd + [point(-2,-2), point(-2,-2), point(-2,-2), point(-2,-2)], rect(((num mod 2)=1)*20,1+b*50,20 + ((num mod 2)=1)*7,1+(b+1)*50), {#ink:36})
        end repeat
      else
        repeat with a = 0 to 5 then
          pstDp = restrict(dp + a, 0, 29)
          member("layer"&string(pstDp)).image.copyPixels(member("largeChainGraf2").image, pastQd, rect(((num mod 2)=1)*20,1+a*50,20 + ((num mod 2)=1)*7,1+(a+1)*50), {#ink:36})
          member("layer"&string(pstDp)).image.copyPixels(member("largeChainGraf2HighLight").image, pastQd + [point(-2,-2), point(-2,-2), point(-2,-2), point(-2,-2)], rect(((num mod 2)=1)*20,1+a*50,20 + ((num mod 2)=1)*7,1+(a+1)*50), {#ink:36})
          
          pstDp = restrict(dp + 4 + a, 0, 29)
          b = 5 - a
          member("layer"&string(pstDp)).image.copyPixels(member("largeChainGraf2").image, pastQd, rect(((num mod 2)=1)*20,1+b*50,20 + ((num mod 2)=1)*7,1+(b+1)*50), {#ink:36})
          member("layer"&string(pstDp)).image.copyPixels(member("largeChainGraf2HighLight").image, pastQd + [point(-2,-2), point(-2,-2), point(-2,-2), point(-2,-2)], rect(((num mod 2)=1)*20,1+b*50,20 + ((num mod 2)=1)*7,1+(b+1)*50), {#ink:36})
        end repeat
      end if  ... 
    
     */

    public class StylegroundChainWireRenderer : BackdropRenderer {
        /*
        public void SegmentProduce(float num, bool LargeChain2, dynamic data, Vector2 pos, Vector2 dir, Vector2 perp, Vector2 lastPos, Vector2 lastDir, Vector2 lastPerp) {
            Vector2 dr = Vector2.Normalize(lastPos-pos) * 11;
            float dst = Vector2.Distance(pos, lastPos);
            float wdth = (num % 2 == 1) ? 3.5f : 10;
            Vector2 pntA = lastPos + dr;
            Vector2 pntB = lastPos - dr;
            Vector2[] pastQuad = new Vector2[4] { pntA - lastPerp * wdth, pntA + lastPerp * wdth, pntB + lastPerp * wdth, pntB - lastPerp * wdth };

            string name = "largeChainGraf" + (LargeChain2 ? "2" : "");
            for(int a = 0; a < 6; a++) {

            }

        }*/
        public class ChainWire {

        }

        public StylegroundChainWireRenderer(EntityData)
    }
}
