import { Component, ElementRef, NgZone, ViewChild } from '@angular/core';

import { SystemRepo } from '@repositories';
import { catchError, finalize, map, switchMap, takeUntil, tap } from 'rxjs/operators';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { NgProgress } from '@ngx-progressbar/core';

import { ActivatedRoute, Params } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SystemConstants } from '@constants';
import { AppForm } from '../app.form';
import { environment } from 'src/environments/environment';
import { GlobalState } from '../global-state';
import { Employee } from '@models';

import UUID from 'validator/lib/isUUID';


declare var $: any;

@Component({
    selector: 'user-profile-page',
    templateUrl: './user-profile.component.html',
    styleUrls: ['./user-profile.component.scss']
})
export class UserProfilePageComponent extends AppForm {
    @ViewChild('image') el: ElementRef;

    currentUserId: string;

    formUser: FormGroup;
    employeeNameVn: AbstractControl;
    employeeNameEn: AbstractControl;
    title: AbstractControl;
    email: AbstractControl;
    bankAccountNo: AbstractControl;
    bankName: AbstractControl;
    tel: AbstractControl;
    description: AbstractControl;
    staffCode: AbstractControl;
    username: AbstractControl;
    workingStatus: AbstractControl;
    creditLimit: AbstractControl;
    creditRate: AbstractControl;

    photoUrl: string;

    constructor(
        private _ngProgressService: NgProgress,
        private _fb: FormBuilder,
        private _systemRepo: SystemRepo,
        private _activedRoute: ActivatedRoute,
        private _toastService: ToastrService,
        private _zone: NgZone,
        private _globalState: GlobalState

    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.initForm();
        this._activedRoute.params
            .pipe(
                takeUntil(this.ngUnsubscribe),
                map((p: Params) => {
                    if (p.id && UUID(p.id)) {
                        return p.id;
                    }
                    return null;
                }),
                tap(id => this.currentUserId = id),
                switchMap((id) => this._systemRepo.getDetailUser(id))
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.setUserForForm(res.data);
                    }
                }
            );
    }

    ngAfterViewInit() {
        this.initImageLibary();
    }

    initImageLibary() {
        this._zone.run(() => {
            $(this.el.nativeElement).froalaEditor({
                requestWithCORS: true,
                language: 'vi',
                imageEditButtons: ['imageReplace'],
                imageMaxSize: 5 * 1024 * 1024,
                imageAllowedTypes: ['jpeg', 'jpg', 'png'],
                requestHeaders: {
                    Authorization: `Bearer ${localStorage.getItem(SystemConstants.ACCESS_TOKEN)}`,
                    Module: 'User',
                    ObjectId: `${this.currentUserId}`,
                },
                imageUploadURL: `//${environment.HOST.SYSTEM}/api/v1/1/SysImageUpload/image`,
                imageManagerLoadURL: `//${environment.HOST.SYSTEM}/api/v1/1/SysImageUpload/User?userId=${this.currentUserId}`,

            }).on('froalaEditor.contentChanged', (e: any) => {
                this.photoUrl = e.target.src;
            }).on('froalaEditor.image.error', (e, editor, error, response) => {
                console.log(error);
                switch (error.code) {
                    case 5:
                        this._toastService.error("Size image invalid");
                        break;
                    case 6:
                        this._toastService.error("Image invalid");
                        break;
                    default:
                        this._toastService.error(error.message);
                        break;
                }
            });
        });
    }

    initForm() {
        this.formUser = this._fb.group({
            employeeNameVn: ['',
                Validators.compose([
                    Validators.required
                ])],
            employeeNameEn: ['',
                Validators.compose([
                    Validators.required
                ])],
            title: [],
            email: ['',
                Validators.compose([
                    Validators.required, Validators.pattern(SystemConstants.CPATTERN.EMAIL)
                ])],
            bankAccountNo: [],
            bankName: [],
            tel: [],
            description: [],

            // view only
            staffCode: [],
            username: [],
            workingStatus: [],
            creditLimit: [],
            creditRate: [],
        });
        //
        this.employeeNameVn = this.formUser.controls['employeeNameVn'];
        this.employeeNameEn = this.formUser.controls['employeeNameEn'];
        this.title = this.formUser.controls['title'];
        this.email = this.formUser.controls['email'];
        this.bankAccountNo = this.formUser.controls['bankAccountNo'];
        this.bankName = this.formUser.controls['bankName'];
        this.tel = this.formUser.controls['tel'];
        this.description = this.formUser.controls['description'];

        // view only
        this.staffCode = this.formUser.controls['staffCode'];
        this.username = this.formUser.controls['username'];
        this.workingStatus = this.formUser.controls['workingStatus'];
        this.creditLimit = this.formUser.controls['creditLimit'];
        this.creditRate = this.formUser.controls['creditRate'];

    }

    setUserForForm(body: any) {
        this.formUser.patchValue({
            employeeNameVn: body.employeeNameVn,
            employeeNameEn: !!body.sysEmployeeModel ? body.sysEmployeeModel.employeeNameEn : null,
            title: !!body.sysEmployeeModel ? body.sysEmployeeModel.title : null,
            email: !!body.sysEmployeeModel ? body.sysEmployeeModel.email : null,
            bankAccountNo: !!body.sysEmployeeModel ? body.sysEmployeeModel.bankAccountNo : null,
            bankName: !!body.sysEmployeeModel ? body.sysEmployeeModel.bankName : null,
            tel: !!body.sysEmployeeModel ? body.sysEmployeeModel.tel : null,
            description: body.description,
            staffCode: !!body.sysEmployeeModel ? body.sysEmployeeModel.staffCode : null,
            username: body.username,
            workingStatus: body.workingStatus,
            creditLimit: body.creditLimit,
            creditRate: body.creditRate,
        });
        this.photoUrl = body.avatar;
    }
    handleUpdateUser() {
        const form = this.formUser.getRawValue();
        if (this.formUser.invalid) {
            return;
        }
        const body = {
            employeeNameVn: form.employeeNameVn,
            employeeNameEn: form.employeeNameEn,
            title: !form.title ? '' : form.title,
            email: form.email,
            bankAccountNo: !form.bankAccountNo ? '' : form.bankAccountNo,
            bankName: !form.bankName ? '' : form.bankName,
            tel: !form.tel ? '' : form.tel,
            description: !form.description ? '' : form.description,
            avatar: this.photoUrl
        };
        this.onUpdate(body);
    }

    onUpdate(body: any) {
        this._progressRef.start();
        this._systemRepo.updateProfile(body)
            .pipe(catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe((body: CommonInterface.IResult) => {
                if (body.status) {
                    this._toastService.success("Upload profile successful");
                    this._globalState.notifyDataChanged('profile', body.data as Employee);
                } else {
                    this._toastService.error("Upload profile fail");
                }
            });
    }
}
