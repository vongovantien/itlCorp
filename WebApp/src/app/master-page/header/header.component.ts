import { Component, OnInit, Input, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { SystemConstants } from 'src/constants/system.const';
import { User, Office } from 'src/app/shared/models';
import { SystemRepo } from '@repositories';
import { Observable, pipe, forkJoin } from 'rxjs';
import { map, tap } from 'rxjs/operators';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html'
})
export class HeaderComponent implements OnInit, AfterViewInit {

    @Input() Page_Info: String;

    english_flag = "assets/app/media/img/lang/en.png";
    vietnam_flag = "assets/app/media/img/lang/vi.png";
    active_flag = "assets/app/media/img/lang/en.png";

    currenUser: SystemInterface.IClaimUser;

    offices: Office[];
    selectedOffice: Office;

    departmentGroups: IDepartmentGroup[] = [];
    selectedDepartmentGroup: IDepartmentGroup;

    constructor(
        private router: Router,
        private _systemRepo: SystemRepo
    ) { }

    ngOnInit() {
        this.currenUser = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        if (!!this.currenUser) {
            forkJoin([
                this._systemRepo.getOfficePermission(this.currenUser.userName, this.currenUser.companyId),
                this._systemRepo.getDepartmentGroupPermission(this.currenUser.userName, this.currenUser.officeId)
            ]).pipe(
                tap((res: any) => {
                    console.log(res[0]);
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
                        this.departmentGroups = res[1];
                        this.selectedDepartmentGroup = this.departmentGroups[0];
                        console.log(res);
                    }
                }
            );
        }
    }

    changeOffice(office: Office) {
        if (!!office) {
            if (this.selectedOffice.id !== office.id) {
                this.selectedOffice = office;
            }
        }
    }

    changeDepartmentGroup(departmentGroup: IDepartmentGroup) {
        if (!!departmentGroup) {
            if (this.selectedDepartmentGroup.groupId !== departmentGroup.groupId) {
                this.selectedDepartmentGroup = departmentGroup;
            }
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

interface IDepartmentGroup {
    userId: string;
    departmentId: number;
    groupId: number;
    departmentName: string;
    groupName: string;
}
