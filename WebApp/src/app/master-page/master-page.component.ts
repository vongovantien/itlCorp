import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { PageSidebarComponent } from './page-sidebar/page-sidebar.component';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { Office } from '@models';

import { BaseService } from 'src/app/shared/services/base.service';
import { environment } from 'src/environments/environment';
import { IAppState } from '../store/reducers';
import { ChangeOfficeClaimUserAction, ChangeDepartGroupClaimUserAction } from '@store';
import { SystemConstants } from 'src/constants/system.const';
import { RSAHelper } from 'src/helper/RSAHelper';
import { CookieService } from 'ngx-cookie-service';
import crypto_js from 'crypto-js';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';


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

    password: string;
    username: string;
    isChangeOffice: boolean = false;
    isChangeDepartgroup: boolean = false;

    ngAfterViewInit(): void {
        this.Page_Info = this.Page_side_bar.Page_Info;
    }

    constructor(
        private baseServices: BaseService,
        private router: Router,
        private cdRef: ChangeDetectorRef,
        private oauthService: OAuthService,
        private http: HttpClient,
        private cookieService: CookieService,
        private _toastService: ToastrService,
        private _spinner: NgxSpinnerService,
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
        this.password = this.getUserPassword();
        this.username = this.getUserName();

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

    officeChange(office: Office) {
        if (!!office) {
            this.selectedOffice = office;
        }
    }

    groupDepartmentChange(groupDepartment: SystemInterface.IDepartmentGroup) {
        if (!!groupDepartment) {
            this.selectedDepartGroup = groupDepartment;
        }
    }

    submitChangeOffice() {
        if (!!this.selectedOffice) {
            const userInfoCurrent: SystemInterface.IClaimUser = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));

            if (!!userInfoCurrent) {
                this.isChangeOffice = true;
                this.loginAgain(userInfoCurrent.companyId, this.selectedOffice.id, userInfoCurrent.departmentId, userInfoCurrent.groupId);
            }
        }
    }

    submitChangedDepartGroup() {
        if (!!this.selectedDepartGroup) {
            const userInfoCurrent: SystemInterface.IClaimUser = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));

            if (!!userInfoCurrent) {
                this.isChangeDepartgroup = true;
                this.loginAgain(userInfoCurrent.companyId, userInfoCurrent.officeId, this.selectedDepartGroup.departmentId, this.selectedDepartGroup.groupId);
            }
        }
    }

    loginAgain(companyId: string, officeId: string, departmentId: number, groupId: number) {
        this._spinner.show();

        this.oauthService.loadDiscoveryDocument().then((a) => {
            this.oauthService.tryLogin().then((b) => {
                const header: HttpHeaders = new HttpHeaders({
                    companyId: companyId,
                    officeId: officeId,
                    groupId: '' + groupId,
                    departmentId: '' + departmentId
                });

                if (!!this.username && !!this.password) {
                    this.oauthService.fetchTokenUsingPasswordFlow(this.username, RSAHelper.serverEncode(this.password), header)
                        .then((tokenInfo: SystemInterface.IToken) => {
                            localStorage.setItem(SystemConstants.ACCESS_TOKEN, tokenInfo.access_token);
                            return this.oauthService.loadUserProfile();
                        }).then((userInfo: SystemInterface.IClaimUser) => {
                            this._spinner.hide();
                            localStorage.setItem(SystemConstants.USER_CLAIMS, JSON.stringify(userInfo));
                            if (userInfo) {
                                if (this.isChangeDepartgroup) {
                                    this._store.dispatch(new ChangeDepartGroupClaimUserAction({ departmentId: this.selectedDepartGroup.departmentId, groupId: this.selectedDepartGroup.groupId }));
                                    this._toastService.info(userInfo.userName.toUpperCase(), "Change Department - Group Success");
                                } else {
                                    this._store.dispatch(new ChangeOfficeClaimUserAction(this.selectedOffice.id));
                                    this._toastService.info(userInfo.userName.toUpperCase(), "Change Office Success");
                                }
                            }
                        });
                }
            }).catch(
                (err) => {
                    this._toastService.error(err + '');
                    this._spinner.hide();
                });
        });
    }

    getUserPassword() {
        const pass_encypt = this.cookieService.get("__p");
        const bytes = crypto_js.AES.decrypt(pass_encypt, SystemConstants.SECRET_KEY);
        return bytes.toString(crypto_js.enc.Utf8);
    }

    getUserName() {
        const username_encypt = this.cookieService.get("__u");
        const bytes = crypto_js.AES.decrypt(username_encypt, SystemConstants.SECRET_KEY);
        return bytes.toString(crypto_js.enc.Utf8);
    }
}
