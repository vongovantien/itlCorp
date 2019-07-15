import { Component, OnInit, Output, EventEmitter, AfterViewInit } from '@angular/core';
import { Router, NavigationEnd, NavigationStart, } from '@angular/router';
import { language } from 'src/languages/language.en';
import { BaseService } from 'src/services-base/base.service';
import $ from 'jquery';

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
    this.highLightMenu();
  }

  public highLightMenu() {
    var router = this.router.url.split('/');
    var current_child_route = router[router.length - 1];
    var parentInd = null;
    var childInd = null;
    var child_name = null;

    for (var i = 0; i < this.Menu.length; i++) {
      for (var j = 0; j < this.Menu[i].childs.length; j++) {

        if (router.includes(this.Menu[i].childs[j].route_child)) {
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


  constructor(private router: Router) {

    this.router.events.subscribe(
      (event) => {
        if (event instanceof NavigationStart) {
          // start loading pages
        }
        if (event instanceof NavigationEnd) {          
          // end of loading paegs
          setTimeout(() => {
            //  this.highLightMenu(); 
          }, 100);
        }
      });
  }

  async ngOnInit() {
    this.Menu = language.Menu;
  }

  /**
   * MENU COMPONENTS DEFINITION
   */
  open_sub_menu(index: number) {
 
    /**
     * Close current parent group
     */
    if (this.previous_menu_index != null) {
      var previous_menu = document.getElementById('parent-'+this.previous_menu_index.toString());
      if (index != this.previous_menu_index) {
        previous_menu.classList.remove('m-menu__item--open');  
      }
    }

    this.previous_menu_index = index;
    this.index_parrent_menu = index;

    /**
     * If parent group is closing then open but close 
     */
    var parentMenu = document.getElementById('parent-'+index.toString());
    if (parentMenu.classList.contains('m-menu__item--open')) {
      parentMenu.classList.remove('m-menu__item--open');
    } else {
      parentMenu.classList.add('m-menu__item--open');
    }
       
  }

  sub_menu_click(sub_menu_name: string, parrent_index: number, children_index: number) {
    var current_parent = document.getElementById('parent-'+parrent_index.toString());
    var current_children = document.getElementById('children-'+parrent_index.toString() + '-' + children_index.toString());

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
    this.router.navigate(['/home/operation/job-management', { action: "create_job" }]);
    this.open_sub_menu(1);
    setTimeout(() => {
      this.sub_menu_click('Job Management', 1, 0);
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
