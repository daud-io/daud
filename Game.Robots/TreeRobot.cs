namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Behaviors.Blending;
    using Game.Robots.Senses;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Numerics;
    using Game.Robots.Models;

    public class TreeRobot : ContextRobot
    {
     

        public int MaxSearch =10;
        public int JumpMS = 50;
        public int Depth = 5;
    public List<API.Client.Body> DangerousBullets;
    public List<TreeState> PathV;
      public List<TreeState> firstRow;

        

        private void Sense()
        {
            foreach (var sensor in Sensors)
                sensor.Sense();
        }
        public (float,float) Score(TreeState t){
            var s=0.0f;
            


            var Projections = DangerousBullets;//.Select(b => b.ProjectNew(this.GameTime + t.OffsetMS).Position).ToList();
            var PhantomProjections = new List<Vector2>();
            var muchFleets = this.SensorFleets.Others
                    .Select(f => new { Fleet = f, Distance = Vector2.Distance(t.Fleet.Center, f.Center) })
                   // .Where(p => MathF.Abs(p.Fleet.Center.X - t.Fleet.Center.X) <= ViewportCrop.X
                  //      && MathF.Abs(p.Fleet.Center.Y - t.Fleet.Center.Y) <= ViewportCrop.Y)
                    .Where(p => !this.HookComputer.Hook.TeamMode || p.Fleet.Color != this.Color);
           
            var leftShips=new List<Ship>();
            foreach (var ship in t.Fleet.Ships)
                 {
                     var worst=float.MaxValue;
            foreach(var v in Projections){
                var diff=(v.ProjectNew(this.GameTime + t.OffsetMS).Position-ship.Position).Length();//RoboMath.ProjectClosestIntersectionDist(this.HookComputer,v,-t.Fleet.Center+t.OldFleet.Center+ship.Position,ship.Position,t.LocalOffsetMS,this);
                // var diff=v-ship.Position;
                    worst=MathF.Min(diff,worst);
            }
             foreach (var flet in muchFleets)
            {
                 //Projections.Append(RoboMath.ProjectClosest( this.HookComputer, flet.Fleet.Center, t.Fleet.Center, t.OffsetMS, flet.Fleet.Ships.Count()));

                foreach (var ship2 in flet.Fleet.Ships)
                {
                    var diff=(ship.Position-ship2.Position).Length()-300.0f;//RoboMath.ProjectClosest(this.HookComputer, ship2.Position, this.Position, t.OffsetMS, flet.Fleet.Ships.Count())).Length();
                // var diff=v-ship.Position;
                    worst=MathF.Min(diff,worst);
                   //Projections.Add(RoboMath.ProjectClosest(this.HookComputer, ship.Position, this.Position, t.OffsetMS, flet.Fleet.Ships.Count()));
                }
            }
            var willAdd=true;
            if(worst<1000.0f){
                        s+=-1.0f/MathF.Max(worst/90.0f,1.0f)/t.Fleet.Ships.Count;
                        if(worst<90.0f){
                            willAdd=false;
                        }
                        if(worst<0.0f){
                            s+=-1.0f/MathF.Max(worst/90.0f,1.0f)/t.Fleet.Ships.Count;
                        }
                    }
                    if(willAdd){
                        leftShips.Add(ship);
                    }
                 }
             float accumulator = 0f;

            var fleet = t.Fleet;
            if (fleet != null)
            {
                var oobX = (MathF.Abs(t.Fleet.Center.X) - this.WorldSize);
                var oobY = (MathF.Abs(t.Fleet.Center.Y) - this.WorldSize);

                if (oobX > -2000.0f)
                    accumulator -= 1.0f/(MathF.Max(-oobX,1.0f));///(MathF.Max(-oobX,1.0f));
                if (oobY > -2000.0f)
                    accumulator -= 1.0f/(MathF.Max(-oobY,1.0f));///(MathF.Max(-oobY,1.0f));
                if (oobX > -1000.0f)
                    accumulator -= oobX+1000.0f;
                if (oobY > -1000.0f)
                    accumulator -= oobY+1000.0f;

            }
    //accumulator=MathF.Max(accumulator,-1.0f);
           s+=accumulator;//*this.SensorFleets.MyFleet.Ships.Count;
           t.Fleet.Ships=leftShips;
        //    if(accumulator<-0.5){
        //        t.Fleet.Ships=new List<Ship>();
        //    }
            return (s+((float)t.Fleet.Ships.Count-(float)this.SensorFleets.MyFleet.Ships.Count),s-accumulator);
        }

        private void Behave()
        {
            Random r=new Random();
            //this.JumpMS=(int)(r.NextDouble()*100.0)+100;
            this.Steps=6;
            // System.Console.WriteLine(this.Steps);
            if (this.SensorFleets.MyFleet != null)
            {
                var teamMode = this.HookComputer.Hook.TeamMode;
                this.DangerousBullets = this.SensorBullets.VisibleBullets
                .Where(b => b.Group.Owner != this.FleetID)
                .Where(b => !teamMode || b.Group.Color != this.Color)
                .ToList();
                var baseS = new TreeState(0, this.SensorFleets.MyFleet, this);
                (var ddd,var qq)=this.Score(baseS);
                baseS.Score=ddd;
                if(qq<-0.5f){
                    //Boost();
                }
                if(qq<=-1.0f){
                    //Boost();
                    System.Console.WriteLine("OOOF: "+qq.ToString());
                }
                var searchPaths = new List<TreeState>{baseS};
                var newSearchPaths = new List<TreeState>();
                
                var searched = 0;

                for(var i=0;i<this.Depth;i++){
                    newSearchPaths = new List<TreeState>();
                    for (int j = 0; j < Math.Min(searchPaths.Count,MaxSearch); j++){
                        if(searchPaths[j].Fleet.Ships.Count>0 && searchPaths[j].Children.Count<1){
                        for (int k = 0; k < this.Steps; k++)
                        {
                            // if(searchPaths[j].Fleet.Ships.Count>0 && searchPaths[j].Children.Count<1){
                            var p=searchPaths[j].ProjectClone(JumpMS*( 1+i),  ((float)k)*360.0f / ((float)this.Steps),k>=this.Steps);
                            (var md,var xd)=this.Score(p);
                            p.Score=md;
                            //if(p.Score>-0.1f){
                                newSearchPaths.Add(p);
                            //}
                            }
                        }else{
                            searchPaths.RemoveAt(j);
                            j--;
                        }
                    }
                    if(i==0){
                        firstRow=newSearchPaths.ToList();
                    }
                    
                    searchPaths=(newSearchPaths.ToList()).OrderByDescending(p=>p.Score+r.NextDouble()*0.0f).ToList();//.Take(i==0?Steps+1:MaxSearch).ToList();
                }
                var contexts = ContextBehaviors.Select(b => b.Behave(Steps)).ToList();
                (var finalRing, var angle, var boost) = ContextRingBlending.Blend(contexts, false);
                BlendedRing = finalRing;
                (var bestp,var bestc)=baseS.bestChildScorePath();
                
                if(bestp.Last().Fleet.Ships.Count*2<=this.SensorFleets.MyFleet.Ships.Count &&CanBoost){
                    //Boost();
                    //System.Console.WriteLine("BOOST");
                }
                //Boost();
                //System.Console.WriteLine("SC: "+bestc+", Ships: "+bestp.Last().Fleet.Ships.Count);
                // System.Console.WriteLine(bestp.Select(x=>x.Angle).ToArray()[1]);
                
                Vector2 angleM=new Vector2(MathF.Cos(GameTime*10.0f),MathF.Sin(GameTime*10.0f));
                float cangle=MathF.Atan2(angleM.Y,angleM.X);
                if(bestp.Count>1){
                var bestD=bestp[1].Angle;//bestp[1].Fleet.Center-this.SensorFleets.MyFleet.Center;
                angleM=new Vector2(MathF.Cos(bestD),MathF.Sin(bestD))*20.0f;//+new Vector2(MathF.Cos(angle),MathF.Sin(angle))*1.0f;//-10.0f*this.SensorFleets.MyFleet.Center/this.SensorFleets.MyFleet.Center.Length();///((float)WorldSize);
                }
                

                // var Projections = DangerousBullets.Select(b => b.Position).ToList().Select(b=>(b-this.SensorFleets.MyFleet.Center).Length()).Concat(new List<float>{10000});
                
            if(bestp.Count>1){
                SteerAngle(MathF.Atan2(angleM.Y,angleM.X));
                PathV=bestp;
                //System.Console.WriteLine(PathV.Count);
            }
            }else{
                //System.Console.WriteLine("NJJHDfbhjdsfgyhtdsgfhtyfgdsuygt");
            }
        }
        protected async override Task AliveAsync()
        {
            Sense();

            await this.OnSensors();

            Behave();
            RingDebugExecute();
        }
        class PlotTrace2
        {
            public IEnumerable<float> x { get; set; }
            public IEnumerable<float> y { get; set; }

            public float opacity { get; set; } = 1.0f;
            public string mode { get; set; } = "markers";
            public string type { get; set; } = "scatter";
        }

        protected void RingDebugExecute()
        {
List<Vector2> st=new List<Vector2>();
if(this.PathV!=null){
    for(var i=0;i<this.PathV.Count;i++){
        st.Add((this.PathV[i].Fleet.Center-this.Position)/500.0f);
    }
    // for(var i=0;i<this.DangerousBullets.Count;i++){
    //     st.Add((this.DangerousBullets[i].Position-this.Position)/200.0f);
    // }
}
st.Add((new Vector2(0,0)-this.Position)/1080.0f);
            var traces = new List<PlotTrace2>();


                traces.Add(new PlotTrace2
                {
                    x = st.Select(x=>x.X),
                    y = st.Select(x=>-x.Y)
                });

            this.CustomData = JsonConvert.SerializeObject(new
            {
                plotly = new
                {
                    data = traces,
                    layout= new{
                        paper_bgcolor = "rgba(0,0,0,0)",
                        plot_bgcolor = "rgba(0,0,0,0)",
                        hovermode = false,
                        xaxis=new {
    range= new[]{ -1, 1 }
  },
  yaxis=new {
    range= new[]{ -1, 1 }
  },
                    }
                }
            });
        }
    }
    
}
