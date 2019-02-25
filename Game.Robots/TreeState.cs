namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Behaviors.Blending;
    using Game.Robots.Senses;
    using Game.Robots.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class TreeState
    {
        
        public int OffsetMS { get; set; }
        public Fleet Fleet { get; set; }
        public TreeRobot TRobot { get; set; }
        public List<TreeState> Children =new List<TreeState>();
        public float Score { get; set; }
        public float Angle=45.0f;
        public bool Stay=false;

        public TreeState(int offset,Fleet f,TreeRobot r)
        {
            OffsetMS=offset+0;
            Fleet=f.Clone();
            TRobot=r;
            Score=0;
            Angle=45.0f;
        }
        public TreeState ProjectClone(int time,float angle,bool stay){
            Fleet newF=this.Fleet.Clone();
            var momentum = newF.Momentum;
                        var position = RoboMath.ShipThrustProjection(TRobot.HookComputer,
                            newF.Center,
                            ref momentum,
                            newF.Ships.Count,
                            angle,
                            time
                        );
                        if(!stay){
                        newF.SetMomentumAndPos(position,momentum);
                        }
                        
            TreeState ns=new TreeState(OffsetMS+time,newF,TRobot);
            ns.Angle=angle;
            ns.Stay=stay;
            this.Children.Add(ns);
            return ns;
        }
        public (List<TreeState>,float) bestChildScorePath(){
            if(Children.Count<1){
                var m=new List<TreeState>();
                m.Add(this);
                return (m,Score);
            }
            else{
                var best=Score+0;
                var bestChain=new List<TreeState>();
                bestChain.Add(this);
                var f=0;
                var totC=0.0f;
                
                foreach(var ts in Children){
                    (var b,var c)=ts.bestChildScorePath();
                    totC+=c/Children.Count;
                    if(c>best || f<1){
                        best=c;
                        bestChain=(new List<TreeState>{this}).Concat(b).ToList();
                        f=1;
                    }
                }
                return (bestChain,best);
            }
        }
    }
}
