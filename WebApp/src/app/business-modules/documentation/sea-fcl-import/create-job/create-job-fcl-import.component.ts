import { Component, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { Router } from '@angular/router';
import { CsTransaction } from 'src/app/shared/models';
import { SeaFClImportFormCreateComponent } from '../components/form-create/form-create-sea-fcl-import.component';
import { formatDate } from '@angular/common';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';
import { Container } from 'src/app/shared/models/document/container.model';
import { TransactionTypeEnum } from 'src/app/shared/enums';

@Component({
    selector: 'app-create-job-fcl-import',
    templateUrl: './create-job-fcl-import.component.html',
    styleUrls: ['./create-job-fcl-import.component.scss']
})
export class SeaFCLImportCreateJobComponent extends AppForm {
    @ViewChild(SeaFClImportFormCreateComponent, { static: false }) formCreateComponent: SeaFClImportFormCreateComponent;

    csTransactionModel: CsTransaction = new CsTransaction();
    constructor(
        protected _router: Router,
        protected _documenRepo: DocumentationRepo
    ) {
        super();
    }

    ngOnInit(): void {
    }

    ngAfterViewInit() {

    }

    onSave() {
        const form: any = this.formCreateComponent.formCreate.value.csTransaction;

        const formData = {
            eta: !!form.eta ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            etd: !!form.etd ? formatDate(form.etd.startDate, 'yyyy-MM-dd', 'en') : null,
            serviceDate: !!form.serviceDate ? formatDate(form.serviceDate.startDate, 'yyyy-MM-dd', 'en') : null,

            mawb: form.mawb,
            voyNo: form.voyNo,
            pono: form.pono,
            notes: form.notes,
            personIncharge: 'admin', // TODO Role = CS.
            subColoader: form.subColoader,

            shipmentType: form.shipmentType.value,
            flightVesselName: form.flightVesselName,
            typeOfService: !!form.typeOfService ? form.typeOfService.value : null,
            mbltype: form.mbltype.value,

            agentId: this.formCreateComponent.selectedAgent.value,
            pol: this.formCreateComponent.selectedPortLoading.value,
            pod: this.formCreateComponent.selectedPortDestination.value,
            deliveryPlace: this.formCreateComponent.selectedPortDelivery.value,
            coloaderId: this.formCreateComponent.selectedSupplier.value,
        };

        this.csTransactionModel = new CsTransaction(formData);
        this.csTransactionModel.transactionTypeEnum = TransactionTypeEnum.SeaFCLImport;
        this.csTransactionModel.csMawbcontainers.push(new Container());
        console.log(this.csTransactionModel);

        this._documenRepo.createTransaction(this.csTransactionModel)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: any) => {
                    console.log(res);
                }
            )
        // console.log(this.csTransactionModel);
    }
}
