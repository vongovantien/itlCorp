import { Component, OnInit, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
// import * as lodash from 'lodash';
import findIndex from 'lodash/findIndex';
import filter from 'lodash/filter';

@Component({
  selector: 'app-breadcrumb',
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.scss']
})
export class BreadcrumbComponent implements OnInit, AfterViewInit {

  Menu: { parent_name: string; icon: string; route_parent: string; display_child: boolean; childs: { name: string; "route_child": string; }[]; }[];
  parent_name = null;
  children_name = null;
  ActiveRoute:any[] = [];

  ngAfterViewInit(): void {
    setTimeout(() => {
      var storagedRoutes = localStorage.getItem("ActiveRoute")
      this.ActiveRoute = storagedRoutes==null?[]:JSON.parse(storagedRoutes);
      var currentURLParts = this.router.url.split("/");
      var currentRoute = currentURLParts[currentURLParts.length-1];
      var moduleRoute= this.route.parent.snapshot.data;    
      var componentRoute = this.route.snapshot.data;

      componentRoute.path = currentRoute;

      var indexModule = findIndex(this.ActiveRoute,x=>x.level===moduleRoute.level);
      var indexComponent = findIndex(this.ActiveRoute,x=>x.level===componentRoute.level);

      if(indexModule===-1){
        this.ActiveRoute.push(moduleRoute,componentRoute);
      }else{
        this.ActiveRoute[indexModule] = moduleRoute;
        if(indexComponent!==-1){
          this.ActiveRoute[indexComponent] = componentRoute;
          this.ActiveRoute = filter(this.ActiveRoute,function(o:any){
            return o.level <= componentRoute.level;
          });
        }
        else{
          this.ActiveRoute.push(componentRoute);
        }
      }

      this.ActiveRoute = this.ActiveRoute;
      localStorage.setItem("ActiveRoute",JSON.stringify(this.ActiveRoute));      
    }, 150);
  }

  navigateRoute(index:number){
    if(index!==this.ActiveRoute.length-1 && index!==0){
      var url = encodeURI('/home/'+this.ActiveRoute[0].path+'/'+this.ActiveRoute[index].path);
      this.router.navigateByUrl(url);
    }else{
      return null
    }
  }


  constructor(private route: ActivatedRoute, private router: Router, private cdRef: ChangeDetectorRef) { }

  ngOnInit() {
    
  }
  

}
