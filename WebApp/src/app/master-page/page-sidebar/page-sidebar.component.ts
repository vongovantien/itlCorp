import { Component, OnInit, Output, EventEmitter, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { language } from 'src/languages/language.en';

@Component({
    selector: 'app-page-sidebar',
    templateUrl: './page-sidebar.component.html',
})
export class PageSidebarComponent implements OnInit, AfterViewInit {
    @Output() Page_Information = new EventEmitter<any>();

    index_parrent_menu = 0;
    index_sub_menu = 0;
    previous_menu_id = null;
    previous_menu_index = null;
    previous_parent: HTMLElement = null;
    previous_children: HTMLElement = null;
    Page_Component = "";
    Page_Info = {
        parent: "",
        children: ""
    };

    Menu: { parent_name: string; icon: string; route_parent: string; display_child: boolean; childs: { name: string; "route_child": string; }[]; }[];

    constructor(private router: Router) {
    }

    ngOnInit() {
        this.Menu = language.Menu;
    }

    ngAfterViewInit(): void {
        this.highLightMenu();
    }

    highLightMenu() {
        const router = this.router.url.split('/');
        let parentInd = null;
        let childInd = null;
        let child_name = null;

        for (let i = 0; i < this.Menu.length; i++) {
            for (let j = 0; j < this.Menu[i].childs.length; j++) {

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

    /**
     * MENU COMPONENTS DEFINITION
     */
    open_sub_menu(index: number) {

        /**
         * Close current parent group
         */
        if (this.previous_menu_index != null) {
            const previous_menu = document.getElementById('parent-' + this.previous_menu_index.toString());
            if (index !== this.previous_menu_index) {
                previous_menu.classList.remove('m-menu__item--open');
            }
        }

        this.previous_menu_index = index;
        this.index_parrent_menu = index;

        /**
         * If parent group is closing then open but close 
         */
        const parentMenu = document.getElementById('parent-' + index.toString());
        if (parentMenu.classList.contains('m-menu__item--open')) {
            parentMenu.classList.remove('m-menu__item--open');
        } else {
            parentMenu.classList.add('m-menu__item--open');
        }

    }

    sub_menu_click(sub_menu_name: string, parrent_index: number, children_index: number) {
        const current_parent = document.getElementById('parent-' + parrent_index.toString());
        const current_children = document.getElementById('children-' + parrent_index.toString() + '-' + children_index.toString());

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

        for (let i = 0; i < this.Menu.length; i++) {
            for (let j = 0; j < this.Menu[i].childs.length; j++) {
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
