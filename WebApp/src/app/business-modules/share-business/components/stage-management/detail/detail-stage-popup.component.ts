import { Component, EventEmitter, Input, OnChanges, OnInit, Output } from "@angular/core";
import { AbstractControl, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute } from '@angular/router';
import { DocumentationRepo } from '@repositories';
import { CsTransactionDetail } from './../../../../../shared/models/document/csTransactionDetail';

import { PopupBase } from "src/app/popup.base";
import { Stage, User } from "src/app/shared/models";
import { OperationRepo, SystemRepo } from "src/app/shared/repositories";

import { formatDate } from "@angular/common";
import { ToastrService } from "ngx-toastr";
import { catchError, finalize, takeUntil } from "rxjs/operators";

@Component({
    selector: "detail-stage-popup",
    templateUrl: "./detail-stage-popup.component.html",
    styleUrls: ['./detail-stage-popup.component.scss']
})
export class ShareBusinessStageManagementDetailComponent extends PopupBase implements OnInit, OnChanges {

    @Input() data: Stage = null;
    @Output() onSuccess: EventEmitter<any> = new EventEmitter<any>();

    form: FormGroup;
    stageName: AbstractControl;
    processTime: AbstractControl;
    description: AbstractControl;
    comment: AbstractControl;
    departmentName: AbstractControl;
    status: AbstractControl;
    deadLineDate: AbstractControl;
    hblno: AbstractControl;

    statusStage: string[] = ['InSchedule', 'Processing', 'Done', 'Overdued', 'Pending', 'Deleted'];


    systemUsers: User[] = [];
    selectedMainPersonInCharge: IPersonInCharge = null;
    selectedRealPersonInCharge: IPersonInCharge = null;
    selectedHbl: Partial<CommonInterface.IComboGridData> = {};

    // config for combo gird
    configComboGrid: any = {
        placeholder: 'Please select',
        displayFields: [
            { field: 'username', label: 'UserName' },
            { field: 'employeeNameEn', label: 'FullName' },
        ],
        source: this.systemUsers,
        selectedDisplayFields: ['username'],
    };

    configHbl: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [
            { field: 'hwbno', label: 'HBL No' },
            { field: 'customerName', label: 'Customer Name' },
        ],
        dataSource: [],
        selectedDisplayFields: ['hwbno'],
    };

    isSubmitted: boolean = false;

    constructor(
        private _fb: FormBuilder,
        private _operationRepo: OperationRepo,
        private _toaster: ToastrService,
        private _systemRepo: SystemRepo,
        private _document: DocumentationRepo,
        private _activedRouter: ActivatedRoute

    ) {
        super();
        this.initForm();
    }

    ngOnChanges() {
        if (!!this.data) {
            this.initFormUpdate();
        }
    }

    ngOnInit() {
        this.getListSystemUser();
        this._activedRouter.params.subscribe((res: any) => {
            if (!!res.jobId) {
                this.getHblList(res.jobId);
            }
        });
    }

    initForm() {
        this.form = this._fb.group({
            'stageName': [{ value: '', disabled: true }, Validators.compose([
                Validators.required,
            ])],
            'processTime': [, Validators.compose([
                Validators.min(1)
            ])],
            'departmentName': [{ value: '', disabled: true }, Validators.compose([
                Validators.required,
            ])],
            'description': [''],
            'comment': [''],
            'deadLineDate': [{
                startDate: null,
                endDate: null
            }],
            'status': [this.statusStage[0]],
            'hblno': [null]
        });
        this.stageName = this.form.controls['stageName'];
        this.processTime = this.form.controls['processTime'];
        this.description = this.form.controls['description'];
        this.comment = this.form.controls['comment'];
        this.departmentName = this.form.controls['departmentName'];
        this.deadLineDate = this.form.controls['deadLineDate'];
        this.status = this.form.controls['status'];
        this.hblno = this.form.controls['hblno'];
    }

    initFormUpdate() {
        this.form.setValue({
            stageName: this.data.stageNameEN,
            comment: this.data.comment || '',
            departmentName: this.data.departmentName,
            description: this.data.description || '',
            processTime: this.data.processTime,
            deadLineDate: !!this.data.deadline ? { startDate: new Date(this.data.deadline), endDate: new Date(this.data.deadline) } : null,
            status: this.data.status,
            hblno: this.data.hblno,
        });

        this.selectedMainPersonInCharge = Object.assign({}, { field: 'id', value: this.data.mainPersonInCharge });
        this.selectedRealPersonInCharge = Object.assign({}, { field: 'id', value: this.data.realPersonInCharge });
        this.selectedHbl = Object.assign({}, { field: 'id', value: this.data.hblId });
    }

    onSelectMainPersonIncharge($event: User) {
        this.selectedMainPersonInCharge.value = $event.username;
    }

    onSelectRealPersonIncharge($event: User) {
        this.selectedRealPersonInCharge.value = $event.username;
    }

    onSelectHouseBill($event: CsTransactionDetail) {
        this.selectedHbl.value = $event.id;
        this.hblno.setValue($event.hwbno);
    }


    getHblList(jobId: string) {
        this._document.getHBLOfJob({ jobId: jobId })
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.configHbl.dataSource = res.filter(x => x.hwbno !== 'N/H').concat(res.find(x => x.hwbno === 'N/H') || []);
                },
                () => { }
            );
    }

    onSubmit(form: FormGroup) {
        this.isSubmitted = true;
        if (form.invalid) {
            return;
        }
        if ((form.value.status === 'Pending' || form.value.status === "Deleted") && !form.value.comment) {
            return;
        }
        if (!this.selectedMainPersonInCharge.value || !this.hblno.value && this.stageName.value !== 'Make Advance/ Settlement') {
            return;
        } else {
            const body = {
                id: this.data.id,
                jobId: this.data.jobId,
                hblId: !!this.selectedHbl.value ? this.selectedHbl.value : null,
                hblno: form.value.hblno,
                stageId: this.data.stageId,
                name: this.data.name,
                orderNumberProcessed: this.data.orderNumberProcessed,
                mainPersonInCharge: this.selectedMainPersonInCharge.value || "admin",
                realPersonInCharge: this.selectedRealPersonInCharge.value,
                processTime: form.value.processTime,
                comment: form.value.comment,
                description: form.value.description,
                deadline: !!form.value.deadLineDate.startDate ? formatDate(form.value.deadLineDate.startDate, 'yyyy-MM-ddTHH:mm', 'en') : null,
                status: form.value.status,
                type: this.data.type,
                userCreated: this.data.userCreated,
                datetimeCreated: this.data.datetimeCreated,
            };
            this._operationRepo.updateStageToJob(body).pipe(
                takeUntil(this.ngUnsubscribe),
                catchError(this.catchError),
                finalize(() => { }),
            ).subscribe(
                (res: any) => {
                    if (!res.status) {
                        this._toaster.error(res.message);
                    } else {
                        this.onSuccess.emit();
                        this._toaster.success(res.message);

                        this.hide();
                    }
                },
                // error
                (errs: any) => {
                },
                // complete
                () => {
                    this.isSubmitted = false;
                    this.form.reset();
                    this.onResetValue();
                }
            );
        }
    }

    getListSystemUser() {
        this._systemRepo.getListSystemUser().pipe(
            takeUntil(this.ngUnsubscribe),
            catchError(this.catchError),
            finalize(() => { }),
        ).subscribe(
            (res: any[]) => {
                if (!res) {
                } else {
                    this.systemUsers = res.map((item: any) => new User(item));
                    Object.assign(this.configComboGrid, { source: this.systemUsers });
                }
            },
            // error
            (errs: any) => {
            },
            // complete
            () => { }
        )
    }

    onCancel() {
        this.isSubmitted = false;
        this.hide();
        this.onResetValue();
    }

    onResetValue() {
        this.selectedHbl = {}
        this.hblno.setValue(null);
    }

    onRemoveData(type: string) {
        switch (type) {
            case "hbl":
                this.selectedHbl = {};
                this.hblno.setValue(null)
                break;
            case "main":
                this.selectedMainPersonInCharge.value = null;
                break;
            case "real":
                this.selectedRealPersonInCharge.value = null;
                break;
            case "deadline":
                this.deadLineDate.setValue(null);
                break;
            default:
                break;
        }
    }
}

interface IPersonInCharge {
    field: string;
    value: string;
}
