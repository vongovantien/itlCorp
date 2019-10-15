import { Component, OnInit, Input, AfterViewInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { UserGroup } from 'src/app/shared/models/system/userGroup.model';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Params } from '@angular/router';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'form-user-group',
    templateUrl: './form-user-group.component.html'
})
export class FormUserGroupComponent extends PopupBase implements OnInit, AfterViewInit {
    @Input() title: string;
    @Input() userGroup: UserGroup = null;
    formGroup: FormGroup;
    isSubmitted: boolean = false;

    users: any[] = [];
    levelPemissions: any[] = [];
    positions: any[] = [];
    permissions: any[] = [];

    user: AbstractControl;

    constructor(private _fb: FormBuilder,
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService) {
        super();
    }

    ngOnInit() {
        this.initForm();
    }
    ngAfterViewInit() {
        if (this.userGroup != null) {
            this.setValueFormGroup(this.userGroup);
        }
    }
    initForm() {
        this.formGroup = this._fb.group({
            user: ['', Validators.compose([
                Validators.required,
            ])],
            level: [''],
            position: [''],
            permission: ['']
        });
        this.user = this.formGroup.controls['user'];
    }
    setValueFormGroup(res: any) {
        this.formGroup.setValue({
            user: this.users.filter(i => i.id === res.departmentId)[0] || null
        });
    }
    addUserToGroup() {
        this.isSubmitted = true;
        if (this.formGroup.valid) {
            this._progressRef.start();
            const body: any = {
                groupId: this.userGroup.groupId,
                userId: this.user.value
            };
            this._systemRepo.addUserToGroup(body)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => {
                        this._progressRef.complete();
                        this.isSubmitted = false;
                        this.initForm();
                    })
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, '');
                        } else {
                            this._toastService.error(res.message, '');
                        }
                    }
                );
        }
    }
}
