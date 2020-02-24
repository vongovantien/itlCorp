import { Component, OnInit, Output, EventEmitter, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { SystemConstants } from 'src/constants/system.const';
import { SystemRepo } from '@repositories';
import { Menu } from '@models';
import { Store } from '@ngrx/store';
import { IAppState, getClaimUserOfficeState } from '@store';

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

    Menu: Menu[] = [];
    userLogged: SystemInterface.IClaimUser;

    constructor(
        private router: Router,
        private _systemRepo: SystemRepo,
        private _store: Store<IAppState>
    ) {
    }

    ngOnInit() {
        // TODO Menu tiếng anh - tiếng việt
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this.getMenu(this.userLogged.officeId);

        this._store.select(getClaimUserOfficeState)
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.getMenu(res);
                    }
                }
            );

    }

    getMenu(officeId: string) {
        if (!!this.userLogged) {
            this._systemRepo.getMenu(this.userLogged.id, officeId)
                .subscribe(
                    (res: Menu[]) => {
                        this.Menu = res.map((m: Menu) => new Menu(m));
                        this.highLightMenu();
                    }
                );
        }
    }

    ngAfterViewInit(): void {
        // TODO khi nhấn back thì gọi lại fn HightlightMenu();
    }

    highLightMenu() {
        const router = this.router.url.split('/');
        let parentInd = null;
        let childInd = null;
        let child_name = null;
        for (let i = 0; i < this.Menu.length; i++) {
            for (let j = 0; j < this.Menu[i].subMenus.length; j++) {
                if (router.includes(this.Menu[i].subMenus[j].route)) {
                    this.Page_Info.parent = this.Menu[i].nameEn;
                    this.Page_Info.children = this.Menu[i].subMenus[j].nameEn;
                    this.Page_Information.emit(this.Page_Info);

                    this.open_sub_menu(i);

                    this.Menu[i].displayChild = true;
                    parentInd = i;
                    childInd = j;
                    child_name = this.Menu[i].subMenus[j].nameEn;
                }
            }
        }
        if (parentInd != null && childInd != null) {
            setTimeout(() => {
                this.sub_menu_click(child_name, parentInd, childInd);
            }, 100);
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
        if (!!parentMenu) {
            if (parentMenu.classList.contains('m-menu__item--open')) {
                parentMenu.classList.remove('m-menu__item--open');
            } else {
                parentMenu.classList.add('m-menu__item--open');
            }
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

        // tslint:disable: prefer-for-of
        for (let i = 0; i < this.Menu.length; i++) {
            for (let j = 0; j < this.Menu[i].subMenus.length; j++) {
                if (this.Menu[i].subMenus[j].nameEn === sub_menu_name) {
                    this.Page_Info.parent = this.Menu[i].nameEn;
                    this.Page_Info.children = this.Menu[i].subMenus[j].nameEn;
                    this.Page_Information.emit(this.Page_Info);
                    break;
                }
            }
        }

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
