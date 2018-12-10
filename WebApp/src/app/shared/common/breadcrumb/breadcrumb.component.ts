import { Component, OnInit, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {language} from 'src/languages/language.en';
import * as lodash from 'lodash';

@Component({
  selector: 'app-breadcrumb',
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.scss']
})
export class BreadcrumbComponent implements OnInit, AfterViewInit {
  Menu: { parent_name: string; icon: string; route_parent: string; display_child: boolean; childs: { name: string; "route_child": string; }[]; }[];
  parent_name = null;
  children_name = null;
  ngAfterViewInit(): void {
    this.Menu = language.Menu;
    console.log(this.router.url);
    var _route = "";
    if(this.router.url.includes(";")){
      _route = this.router.url.split(";")[0];
    }else{
      _route = this.router.url;
    }
    var route = _route.split("/");

    var child_path = route[route.length - 1];
    var parent_path = route[route.length - 2];
    var index_module = lodash.findIndex(this.Menu, function (o) {
      var route_module = o.route_parent.split("/");
      return route_module[route_module.length - 2] == parent_path;
    });
    var index_child = lodash.findIndex(this.Menu[index_module].childs, function (o) {
      return child_path == o.route_child;
    });

    this.parent_name = this.Menu[index_module].parent_name;
    this.children_name = this.Menu[index_module].childs[index_child].name;
    this.cdRef.detectChanges();

    // setTimeout(() => {
    //   var route = this.router.url.split("/");
    //   var child_path = route[route.length - 1];
    //   var parent_path = route[route.length - 2];
    //   var index_module = lodash.findIndex(this.Menu, function (o) {
    //     var route_module = o.route_parent.split("/");
    //     return route_module[route_module.length - 2] == parent_path;
    //   });
    //   var index_child = lodash.findIndex(this.Menu[index_module].childs, function (o) {
    //     return child_path == o.route_child;
    //   });
  
    //   this.parent_name = this.Menu[index_module].parent_name;
    //   this.children_name = this.Menu[index_module].childs[index_child].name;
    //   this.cdRef.detectChanges();
  
    // }, 500);
 

  }

  constructor(private route: ActivatedRoute, private router: Router, private cdRef: ChangeDetectorRef) { }

  ngOnInit() {

  }
  

}
