import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { SystemRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { finalize, catchError, takeUntil } from 'rxjs/operators';
import { UserLevel } from 'src/app/shared/models/system/userlevel';
import { SystemConstants } from '@constants';
//SwitchUser
import { HttpHeaders } from '@angular/common/http';
import { OAuthService } from 'angular-oauth2-oidc';
import { NgxSpinnerService } from 'ngx-spinner';
import { RSAHelper } from 'src/helper/RSAHelper';
import { getCurrentUserState, IAppState } from '@store';
import { Store } from '@ngrx/store';

import { CookieService } from 'ngx-cookie-service';
import crypto_js from 'crypto-js';
@Component({
    selector: 'app-form-add-user',
    templateUrl: './form-add-user.component.html'
})
export class FormAddUserComponent extends AppList {
    formGroup: FormGroup;
    isSubmited: boolean = false;
    isDetail: boolean = false;
    SelectedUser: any = {};
    staffcode: AbstractControl;
    username: AbstractControl;
    employeeNameVn: AbstractControl;
    employeeNameEn: AbstractControl;
    title: AbstractControl;
    active: AbstractControl;
    usertype: AbstractControl;
    workingg: AbstractControl;
    email: AbstractControl;
    phone: AbstractControl;
    description: AbstractControl;
    ldap: AbstractControl;
    personalId: AbstractControl;
    userLevels: UserLevel[] = [];
    headersuslv: CommonInterface.IHeaderTable[];
    //
    creditLimit: AbstractControl;
    creditRate: AbstractControl;
    userRole: AbstractControl;
    currentUser: any;
    selectedCompanyId: any;

    status: CommonInterface.ICommonTitleValue[] = [
        { title: 'Active', value: true },
        { title: 'Inactive', value: false },
    ];

    usertypes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Normal User', value: 'Normal User' },
        { title: 'Local Admin', value: 'Local Admin' },
        { title: 'Super Admin', value: 'Super Admin' }
    ];

    working: CommonInterface.ICommonTitleValue[] = [
        { title: 'Working', value: 'Working' },
        { title: 'Maternity leave', value: 'Maternity leave' },
        { title: 'Off', value: 'Off' }
    ];

    userRoles: CommonInterface.ICommonTitleValue[] = [
        {
            "title": "CS Document",
            "value": "CS"
        },
        {
            "title": "Sale",
            "value": "Sale"
        },
        {
            "title": "Accountant",
            "value": "FIN"
        },
        {
            "title": "Internal Audit",
            "value": "IA"
        },
        {
            "title": "Account Receivable",
            "value": "AR"
        },
        {
            "title": "BOD",
            "value": "BOD"
        }
    ];

    constructor(
        private _fb: FormBuilder,
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService,
        private _progressService: NgProgress,
        private _oauthService: OAuthService,
        private _spinner: NgxSpinnerService,
        private _store: Store<IAppState>,
        private cookieService: CookieService,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    initForm() {
        this.formGroup = this._fb.group({
            staffcode: ['',
                Validators.compose([
                    Validators.required
                ])],
            username: ['',
                Validators.compose([
                    Validators.required
                ])],
            employeeNameVn: ['',
                Validators.compose([
                    Validators.required
                ])],
            employeeNameEn: ['',
                Validators.compose([
                    Validators.required
                ])],
            title: [],
            usertype: [this.usertypes[0]],
            active: [this.status[0]],
            workingg: [this.working[0]],
            email: ['', Validators.compose([
                Validators.required,
                Validators.pattern(SystemConstants.CPATTERN.EMAIL),
            ])],
            phone: [],
            description: [],
            ldap: [true],
            //
            creditLimit: [],
            creditRate: [],
            personalId: [],
            userRole: [this.userRoles[0],
            Validators.compose([
                Validators.required
            ])]
        });

        this.staffcode = this.formGroup.controls['staffcode'];
        this.username = this.formGroup.controls['username'];
        this.employeeNameVn = this.formGroup.controls['employeeNameVn'];
        this.employeeNameEn = this.formGroup.controls['employeeNameEn'];
        this.title = this.formGroup.controls['title'];
        this.usertype = this.formGroup.controls['usertype'];
        this.active = this.formGroup.controls['active'];
        this.email = this.formGroup.controls['email'];
        this.phone = this.formGroup.controls['phone'];
        this.description = this.formGroup.controls['description'];
        this.ldap = this.formGroup.controls['ldap'];
        this.workingg = this.formGroup.controls['workingg'];
        //
        this.creditLimit = this.formGroup.controls['creditLimit'];
        this.creditRate = this.formGroup.controls['creditRate'];
        this.personalId = this.formGroup.controls['personalId'];
        this.userRole = this.formGroup.controls['userRole'];

    }

    ngOnInit() {
        this.initForm();
        this.headersuslv = [
            { title: 'Group Name', field: 'groupName' },
            { title: 'Company', field: 'companyName' },
            { title: 'Office', field: 'officeName' },
            { title: 'Department', field: 'departmentName' },
            { title: 'Position', field: 'position' },
        ];
        this._store.select(getCurrentUserState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((res) => {
                this.currentUser = res;
            });
    }

    updateCookies(username: string, password: string) {
        const username_encrypt = crypto_js.AES.encrypt(username, SystemConstants.SECRET_KEY).toString();
        this.cookieService.set("__u", username_encrypt, 1, "/", window.location.hostname);

        const password_encrypt = crypto_js.AES.encrypt(password, SystemConstants.SECRET_KEY).toString();
        this.cookieService.set("__p", password_encrypt, 1, "/", window.location.hostname);
    }

    resetPassword(id: string) {
        this.isLoading = true;
        this._progressRef.start();
        this._systemRepo.resetPasswordUser(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }

    switchUser(user: any) {
        this._oauthService.loadDiscoveryDocument().then((a) => {
            this._oauthService.tryLogin().then((b) => {
                let header: HttpHeaders = new HttpHeaders({
                    companyId: this.currentUser.companyId,
                    userType: 'Super Admin',
                });
                const Username = user.username;
                const Password = '@';
                if (!!Username && !!Password) {
                    this._oauthService.fetchTokenUsingPasswordFlow(Username, RSAHelper.serverEncode(Password), header)
                        .then((tokenInfo: SystemInterface.IToken) => {
                            localStorage.setItem(SystemConstants.ACCESS_TOKEN, tokenInfo.access_token);
                            return this._oauthService.loadUserProfile();
                        }).then((userInfo: SystemInterface.IClaimUser) => {
                            this._spinner.hide();
                            localStorage.setItem(SystemConstants.USER_CLAIMS, JSON.stringify(userInfo));
                            localStorage.setItem(SystemConstants.USER_ACCESS_PERMISSION, 'authorized');
                            this.updateCookies(userInfo.userName, Password);
                            window.location.replace("#/home");
                            window.location.reload();
                        }).catch((err) => {
                            this._spinner.hide();
                        });
                } else {
                    throw new Error("Can't access to user's account, please check credentials");
                }
            }).catch(
                (err) => {
                    this._toastService.error(err + '');
                    this._spinner.hide();
                });
        });
    }


}
