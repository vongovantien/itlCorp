import { catchError, finalize } from 'rxjs/operators';
import { ShareAirServiceFormCreateComponent } from '../../../documentation/share-air/components/form-create/form-create-air.component';
import { CsTransaction } from '@models';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { DocumentationRepo } from '@repositories';
import { Directive, ViewChild } from '@angular/core';
import { JobConstants } from '@constants';
import { ConfirmPopupComponent, SubHeaderComponent } from '@common';
import { AppForm } from 'src/app/app.form';
import * as fromShareBussiness from '../../store';

@Directive()
export class ShareDetailJobComponent extends AppForm {

    @ViewChild(SubHeaderComponent) headerComponent: SubHeaderComponent;

    jobId: string;
    ACTION: CommonType.ACTION_FORM | string = 'UPDATE';
    shipmentDetail: CsTransaction;
    constructor(
        protected _toastService: ToastrService,
        protected _documenRepo: DocumentationRepo,
    ) {
        super();
    }

    getDetailShipment(jobId: string) {

    }

    onFinishJob() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Do you want to finish this shipment ?',
            labelConfirm: 'Yes'
        }, () => {
            this.handleChangeStatusJob(JobConstants.FINISH);
        })
    }

    onReopenJob() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Do you want to reopen this shipment ?',
            labelConfirm: 'Yes'
        }, () => {
            this.handleChangeStatusJob(JobConstants.REOPEN);
        })

    }

    handleChangeStatusJob(status: string) {
        let body: any = {
            jobId: this.jobId,
            transactionType: JobConstants.CSTRANSACTION,
            status
        }
        this._documenRepo.updateStatusJob(body).pipe(
            catchError(this.catchError),
            finalize(() => {
                this._progressRef.complete();
            })
        ).subscribe(
            (r: CommonInterface.IResult) => {
                if (r.status) {
                    this.getDetailShipment(this.jobId);
                    this._toastService.success(r.message);
                } else {
                    this._toastService.error(r.message);
                }
            },
        );
    }
}
