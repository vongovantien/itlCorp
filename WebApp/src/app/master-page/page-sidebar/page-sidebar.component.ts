import { Component, OnInit, Output, EventEmitter, AfterViewInit} from '@angular/core';
import { Router } from '@angular/router';
import {language} from 'src/languages/language.en';
import { BaseService } from 'src/services-base/base.service';

@Component({
  selector: 'app-page-sidebar',
  templateUrl: './page-sidebar.component.html',
  // styleUrls: ['./page-sidebar.component.css']
})
export class PageSidebarComponent implements OnInit, AfterViewInit {  
  index_parrent_menu = 0;
  index_sub_menu = 0;
  previous_menu_id = null;
  previous_menu_index = null;
  previous_parent: HTMLElement = null;
  previous_children: HTMLElement = null;
  //action_component="";
  @Output() Page_Information = new EventEmitter<any>();
  Page_Component = "";
  Page_Info = {
    parent: "",
    children: ""
  }
  Menu: { parent_name: string; icon: string; route_parent: string; display_child: boolean; childs: { name: string; "route_child": string; }[]; }[];
  
  ngAfterViewInit(): void {
    var router = this.router.url.split('/');
    var current_child_route = router[router.length - 1];
    var parentInd = null;
    var childInd = null;
    var child_name = null; 

    for (var i = 0; i < this.Menu.length; i++) {
      for (var j = 0; j < this.Menu[i].childs.length; j++) {

        if (this.Menu[i].childs[j].route_child == current_child_route) {
          this.Page_Info.parent = this.Menu[i].parent_name;
          this.Page_Info.children = this.Menu[i].childs[j].name;         
          this.Page_Information.emit(this.Page_Info);         
          this.open_sub_menu(i);
          this.Menu[i].display_child = true;
          parentInd = i; childInd = j; child_name = this.Menu[i].childs[j].name
        }
      }
    }
    if (parentInd != null && childInd != null) {
      setTimeout(() => {
        this.sub_menu_click(child_name, parentInd, childInd);
      }, 400);
    }

  }
  public highLightMenu(){
    
  }


  constructor(private router: Router,private baseService:BaseService) { }

  async ngOnInit() {
    this.Menu = language.Menu;
  }

  /**
   * MENU COMPONENTS DEFINITION
   */

  
  open_sub_menu(index: number) {
    if (this.previous_menu_index != null) {
      this.Menu[this.previous_menu_index].display_child = false;
      var previous_menu = document.getElementById(this.previous_menu_index.toString());     
      if (index != this.previous_menu_index) {
        previous_menu.classList.remove('m-menu__item--open');
        // var check_class = previous_menu.classList.contains('m-menu__item--open');       
      }

    }
    this.previous_menu_index = index;

    this.index_parrent_menu = index;
    var parentMenu = document.getElementById(index.toString());   
    if (parentMenu.classList.contains('m-menu__item--open')) {
      parentMenu.classList.remove('m-menu__item--open');
    } else {
      parentMenu.classList.add('m-menu__item--open');
    }

    this.Menu[index].display_child = !this.Menu[index].display_child;
    // this.Page_Info.parent= this.Menu[index].parent_name;    
  }

  sub_menu_click(sub_menu_name: string, parrent_index: number, children_index: number) { 
      var current_parent = document.getElementById(parrent_index.toString());
      var current_children = document.getElementById(parrent_index.toString() + '-' + children_index.toString());
  
      if (this.previous_children != null) {
        this.previous_children.classList.remove('m-menu__item--active');
        this.previous_parent.classList.remove('m-menu__item--open');
        this.previous_parent.classList.remove('m-menu__item--active');
      }
  
      this.previous_children = current_children;
      this.previous_parent = current_parent;
  
  
      current_parent.classList.add('m-menu__item--open');
      current_parent.classList.add('m-menu__item--active');
      current_children.classList.add('m-menu__item--active');
  
      
      for (var i = 0; i < this.Menu.length; i++) {
        for (var j = 0; j < this.Menu[i].childs.length; j++) {
          if (this.Menu[i].childs[j].name == sub_menu_name) {
            this.Page_Info.parent = this.Menu[i].parent_name;
            this.Page_Info.children = this.Menu[i].childs[j].name;
            this.Page_Information.emit(this.Page_Info);        
            break;
          }
        }
      }
    
  }

  gotoJobManagement() {
    this.router.navigate(['/home/operation/job-management',{action:"create_job"}]);
    this.open_sub_menu(1);
    setTimeout(() => {
      this.sub_menu_click('Job Management',1,0);
    }, 200);
  }

  mouseenter() {
    document.body.classList.add('body-fixed');
    if (document.body.classList.contains('m-aside-left--minimize')) {
      document.body.classList.remove('m-aside-left--minimize');
      document.body.classList.add('m-aside-left--minimize-hover');
    }
  }  

  mouseleave() {
    document.body.classList.remove('body-fixed');
    if (document.body.classList.contains('m-aside-left--minimize-hover')) {
      document.body.classList.remove('m-aside-left--minimize-hover');
      document.body.classList.add('m-aside-left--minimize');
    }
  }

}
