import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { PageSidebarComponent } from './page-sidebar/page-sidebar.component';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { HttpClient } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { Office } from '@models';

import { BaseService } from 'src/app/shared/services/base.service';
import { environment } from 'src/environments/environment';
import { IAppState } from '../store/reducers';
import { ChangeOfficeClaimUserAction, ChangeDepartGroupClaimUserAction } from '@store';
import { SystemConstants } from 'src/constants/system.const';


@Component({
    selector: 'app-master-page',
    templateUrl: './master-page.component.html',
})
export class MasterPageComponent implements OnInit, AfterViewInit {

    @ViewChild(PageSidebarComponent, { static: false }) Page_side_bar: { Page_Info: {}; };
    Page_Info = {};
    Component_name: "no-name";

    selectedOffice: Office;
    selectedDepartGroup: SystemInterface.IDepartmentGroup;


    ngAfterViewInit(): void {
        this.Page_Info = this.Page_side_bar.Page_Info;
    }

    constructor(
        private baseServices: BaseService,
        private router: Router,
        private cdRef: ChangeDetectorRef,
        private oauthService: OAuthService,
        private http: HttpClient,
        private _store: Store<IAppState>
    ) { }

    ngOnInit() {
        this.cdRef.detectChanges();
        setInterval(() => {
            const remainingMinutes: number = this.baseServices.remainingExpireTimeToken();
            if (remainingMinutes <= 3 && remainingMinutes > 0) {
                this.baseServices.warningToast("Phiên đăng nhập sẽ hết hạn sau " + remainingMinutes + " phút nữa, hãy lưu công việc hiện tại hoặc đăng nhập lại để tiếp tục công việc.", "Cảnh Báo !")
            }
        }, 15000);
    }

    MenuChanged(event: any) {
        this.Page_Info = event;
        this.Component_name = event.children;
    }

    logout() {
        this.http.get(`${environment.HOST.INDENTITY_SERVER_URL}/api/Account/Signout`).toPromise()
            .then(
                (res: any) => {
                    this.oauthService.logOut(false);
                    this.router.navigate(["login"]);
                },
                (error: any) => {
                    console.log(error + '');
                }
            );
    }

    officeChange(office: any) {
        if (office) {
            this.selectedOffice = office;
        }
    }

    groupDepartmentChange(groupDepartment: SystemInterface.IDepartmentGroup) {
        if (groupDepartment) {
            this.selectedDepartGroup = groupDepartment;
        }
    }

    submitChangeOffice() {
        const userInfoCurrent: SystemInterface.IClaimUser = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        userInfoCurrent.officeId = this.selectedOffice.id;

        // * Update LocalStorage
        localStorage.setItem(SystemConstants.USER_CLAIMS, JSON.stringify(userInfoCurrent));

        // * Dispatch action change office to header.
        this._store.dispatch(new ChangeOfficeClaimUserAction(this.selectedOffice.id));
    }

    submitChangedDepartGroup() {
        const userInfoCurrent: SystemInterface.IClaimUser = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        userInfoCurrent.departmentId = this.selectedDepartGroup.departmentId;
        userInfoCurrent.groupId = this.selectedDepartGroup.groupId;

        // * Update LocalStorage
        localStorage.setItem(SystemConstants.USER_CLAIMS, JSON.stringify(userInfoCurrent));

        // * Dispatch action change office to header.
        this._store.dispatch(new ChangeDepartGroupClaimUserAction({ departmentId: this.selectedDepartGroup.departmentId, groupId: this.selectedDepartGroup.groupId }));
    }
}
