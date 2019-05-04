import { Component, OnInit, AfterViewInit, ChangeDetectorRef, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from "@angular/common";
import {language} from 'src/languages/language.en';
import * as lodash from 'lodash';
import uniq from 'lodash/uniq';
import {BreadcrumbData} from './BreadcrumbData';


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
      var currentURLParts = this.router.url.split("/");
     
      var currentRoute = currentURLParts[currentURLParts.length-1];
      var moduleRoute= this.route.parent.snapshot.data;    
      var componentRoute = this.route.snapshot.data;

      componentRoute.path = currentRoute;

      var indexModule = lodash.findIndex(BreadcrumbData.RouteStack,x=>x.level===moduleRoute.level);
      var indexComponent = lodash.findIndex(BreadcrumbData.RouteStack,x=>x.level===componentRoute.level);

      if(indexModule===-1){
        BreadcrumbData.RouteStack.push(moduleRoute,componentRoute);
      }else{
        BreadcrumbData.RouteStack[indexModule] = moduleRoute;
        if(indexComponent!==-1){
          BreadcrumbData.RouteStack[indexComponent] = componentRoute;
          BreadcrumbData.RouteStack = lodash.filter(BreadcrumbData.RouteStack,function(o:any){
            return o.level <= componentRoute.level;
          });
        }
        else{
          BreadcrumbData.RouteStack.push(componentRoute);
        }
      }

      this.ActiveRoute = BreadcrumbData.RouteStack;
      console.log(BreadcrumbData.RouteStack);      
    }, 200);
    



  }

  navigateRoute(index:number){
    if(index!==this.ActiveRoute.length-1 && index!==0){
      var url = encodeURI('/home/'+this.ActiveRoute[0].path+'/'+this.ActiveRoute[index].path);
      this.router.navigateByUrl(url);
    }else{
      return null
    }
  }



  getModuleRouteData(){
    this.Menu = language.Menu;
    var _route = "";
    if(this.router.url.includes(";")){
      _route = this.router.url.split(";")[0];
    }else{
      _route = this.router.url;
    }
    var routes = _route.split("/");
    var module_path = routes[routes.length-2];

  }

  constructor(private location: Location,private route: ActivatedRoute, private router: Router, private cdRef: ChangeDetectorRef) { }

  ngOnInit() {
    
  }
  

}
