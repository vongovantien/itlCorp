import { Component, OnInit, Input, AfterViewInit, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { SystemRepo } from '@repositories';
import { IAppState, getClaimUserOfficeState, getClaimUserDepartGrouptate } from '@store';

import { SystemConstants } from 'src/constants/system.const';
import { Office } from 'src/app/shared/models';
import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html'
})
export class HeaderComponent implements OnInit, AfterViewInit {

    @Input() Page_Info: String;
    @Output() officeChange: EventEmitter<Office> = new EventEmitter<Office>();
    @Output() groupDepartmentChange: EventEmitter<SystemInterface.IDepartmentGroup> = new EventEmitter<SystemInterface.IDepartmentGroup>();


    english_flag = "assets/app/media/img/lang/en.png";
    vietnam_flag = "assets/app/media/img/lang/vi.png";
    active_flag = "assets/app/media/img/lang/en.png";

    currenUser: SystemInterface.IClaimUser;

    offices: Office[];
    selectedOffice: Office;

    departmentGroups: SystemInterface.IDepartmentGroup[] = [];
    selectedDepartmentGroup: SystemInterface.IDepartmentGroup;

    constructor(
        private router: Router,
        private _systemRepo: SystemRepo,
        private _store: Store<IAppState>
    ) { }

    ngOnInit() {
        this._store.select(getClaimUserOfficeState)
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.selectedOffice = this.offices.find(o => o.id === res);
                    }
                }
            );

        this._store.select(getClaimUserDepartGrouptate)
            .subscribe(
                (res: any) => {
                    if (!!res && res.departmentId && res.groupId) {
                        console.log("depart-group in redux", res);
                        this.selectedDepartmentGroup = this.departmentGroups.find(d => d.departmentId === res.departmentId && d.groupId === res.groupId);
                    }
                }
            );

        this.currenUser = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        if (!!this.currenUser) {
            forkJoin([
                this._systemRepo.getOfficePermission(this.currenUser.userName, this.currenUser.companyId),
                this._systemRepo.getDepartmentGroupPermission(this.currenUser.userName, this.currenUser.officeId)
            ]).pipe(
                tap((res: any) => {
                    this.offices = res[0] || [];
                    if (!!this.offices.length) {
                        if (this.offices.length === 1) {
                            this.selectedOffice = this.offices[0];
                        } else {
                            this.selectedOffice = this.offices.find(o => o.id === this.currenUser.officeId);
                        }
                    }
                })
            ).subscribe(
                (res: any) => {
                    if (!!res) {
                        this.departmentGroups = res[1] || [];
                        if (!!this.departmentGroups.length) {
                            if (this.departmentGroups.length === 1) {
                                this.selectedDepartmentGroup = this.departmentGroups[0];
                            } else {
                                this.selectedDepartmentGroup = this.departmentGroups.find(d => d.departmentId === +this.currenUser.departmentId && d.groupId === +this.currenUser.groupId);
                            }
                        }
                    }
                }
            );
        }
    }

    changeOffice(office: Office) {
        if (!!office) {
            this.officeChange.emit(office);
        }
    }

    changeDepartmentGroup(departmentGroup: SystemInterface.IDepartmentGroup) {
        if (!!departmentGroup) {
            this.groupDepartmentChange.emit(departmentGroup);
        }
    }

    getCurrentLangFromUrl() {
        const url = window.location.href;
        const host = window.location.hostname;
        const url_arr = url.split("/");
        const current_lang_index = url_arr.indexOf(host) + 1;
        return url_arr[current_lang_index];
    }

    ngAfterViewInit(): void {
        if (this.getCurrentLangFromUrl() === "en") {
            this.active_flag = this.english_flag;
            localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, "en");
            localStorage.setItem(SystemConstants.CURRENT_LANGUAGE, "en-US");
        }
        if (this.getCurrentLangFromUrl() === "vi") {
            this.active_flag = this.vietnam_flag;
            localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, "vi");
            localStorage.setItem(SystemConstants.CURRENT_LANGUAGE, "vi-VN");
        }
    }

    changeLanguage(lang: string) {
        if (lang === localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE)) {
            return;
        } else {
            if (lang === "en") {
                localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, SystemConstants.LANGUAGES.ENGLISH);
                localStorage.setItem(SystemConstants.CURRENT_LANGUAGE, SystemConstants.LANGUAGES.ENGLISH_API);
                localStorage.setItem(SystemConstants.CURRENT_LANGUAGE, "en-US");
                const url = window.location.protocol + "//" + window.location.hostname + "/" + lang + "/#" + this.router.url + "/";
                this.active_flag = this.english_flag;
                window.location.href = url;
            }
            if (lang === "vi") {
                localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, SystemConstants.LANGUAGES.VIETNAM);
                localStorage.setItem(SystemConstants.CURRENT_LANGUAGE, SystemConstants.LANGUAGES.VIETNAM_API);
                localStorage.setItem(SystemConstants.CURRENT_LANGUAGE, "vi-VN");
                const url = window.location.protocol + "//" + window.location.hostname + "/" + lang + "/#" + this.router.url + "/";
                this.active_flag = this.vietnam_flag;
                window.location.href = url;
            }
        }
    }


    // minimize sidebar
    minimize_page_sidebar() {
        const bodyElement = document.getElementById('bodyElement');
        const leftMinimizeToggle = document.getElementById('m_aside_left_minimize_toggle');
        if (leftMinimizeToggle.classList.contains('m-brand__toggler--active')) {
            bodyElement.classList.remove('m-brand--minimize', 'm-aside-left--minimize');
            leftMinimizeToggle.classList.remove('m-brand__toggler--active');
        } else {
            bodyElement.classList.add('m-brand--minimize', 'm-aside-left--minimize');
            leftMinimizeToggle.classList.add('m-brand__toggler--active');
        }
    }
}


