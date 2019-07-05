import { Component, OnInit, Input } from "@angular/core";
import moment from "moment/moment";
import { PopupBase } from "src/app/modal.base";
import { FormBuilder, FormGroup, AbstractControl, Validators } from "@angular/forms";
import { Stage } from "src/app/shared/models/operation/stage";
import { JobRepo } from "src/app/shared/repositories";
import { takeUntil, catchError, finalize } from "rxjs/operators";
import { ToastrService } from "ngx-toastr";

@Component({
    selector: "detail-stage-popup",
    templateUrl: "./detail-stage-popup.component.html"
})
export class OpsModuleStageManagementDetailComponent extends PopupBase implements OnInit {

    @Input() data: Stage = null;

    form: FormGroup;
    stageName: AbstractControl;
    processTime: AbstractControl;
    description: AbstractControl;
    comment: AbstractControl;
    departmentName: AbstractControl;

    deadLineDate: any;

    statusStage: Array<string> = ["In schedule", "Processing", "Done", "Overdue", "Pending", "Deleted"];

    //config for combo gird
    configComboGrid: any = {
        user: {
            placeholder: 'Please select',
            displayFields: [
                { field: 'username', label: 'UserName' },
                { field: 'employeeNameEn', label: 'FullName' },
                { field: 'id', label: 'Role' }],
            source: [
                { id: 'Admin', username: 'Admin', employeeNameEn: 'Kenny.Thương' },
                { id: 'Admin 2', username: 'Admin 2', employeeNameEn: 'Kenny' },
                { id: 'Admin 3', username: 'Admin 3', employeeNameEn: 'Thương' },

            ],
            selectedDisplayFields: ['username'],
        }
    }
    statusStageActive = ["In schedule"];

    value: any = {};

    constructor(
        private _fb: FormBuilder,
        private _jobRepo: JobRepo,
        private _toaster: ToastrService
    ) {
        super();
        this.initForm();
    }

    ngOnChanges() {
        if (!!this.data) {
            this.initFormUpdate();
            console.log(this.data);
        }
    }

    ngOnInit() {
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
            'description': ['',],
            'comment': ['',],
        });
        this.stageName = this.form.controls['stageName'];
        this.processTime = this.form.controls['processTime'];
        this.description = this.form.controls['description'];
        this.comment = this.form.controls['comment'];
        this.departmentName = this.form.controls['departmentName'];
    }

    initFormUpdate() {
        this.form.setValue({
            stageName: this.data.stageNameEN,
            comment: this.data.comment || '',
            departmentName: this.data.departmentName,
            description: this.data.description || '',
            processTime: this.data.processTime,
        });
        this.deadLineDate = this.data.deadline;
    }

    selected(value: any): void {
        console.log("Selected value is: ", value);
    }

    removed(value: any): void {
        console.log("Removed value is: ", value);
    }

    typed(value: any): void {
        console.log("New search input: ", value);
    }

    refreshValue(value: any): void {
        this.value = value;
    }

    onSelectMainPersonIncharge($event: any) {
        console.log($event);
    }

    onSelectRealPersonIncharge($event) {
        console.log($event);
    }

    onSubmit(form: FormGroup) {
        const body = {
            id: this.data.id,
            jobId: this.data.jobId,
            stageId: this.data.stageId,
            name: this.data.name,
            orderNumberProcessed: this.data.orderNumberProcessed,
            mainPersonInCharge: this.data.mainPersonInCharge || "kenny",
            realPersonInCharge: this.data.realPersonInCharge || "kenny",
            processTime: form.value.processTime,
            comment: form.value.comment,
            description: form.value.description,
            deadline: moment(this.deadLineDate).format("YYYY-MM-DDTHH:mm:ssZ"),
            status: 'Pending'
        }
        console.log(body);
        this._jobRepo.updateStageToJob(body).pipe(
            takeUntil(this.ngUnsubscribe),
            catchError(this.catchError),
            finalize(() => { }),
        ).subscribe(
            (res: any) => {
                if (!res.status) {
                    this._toaster.success(res.message, '', { positionClass: 'toast-bottom-right' });
                } else {
                    console.log(res);
                    // remove stages selected
                    this.hide();
                }
            },
            // error
            (errs: any) => {
                // this.handleErrors(errs)
            },
            // complete
            () => { }
        )
    }
}
