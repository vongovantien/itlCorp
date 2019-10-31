import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'app-create-house-bill',
    templateUrl: './create-house-bill.component.html',
    styleUrls: ['./create-house-bill.component.scss']
})
export class CreateHouseBillComponent implements OnInit {
    formGroup: FormGroup;
    constructor(
        private _fb: FormBuilder,
    ) { }
    ngOnInit() {
        this.initForm();
    }

    initForm() {
        this.formGroup = this._fb.group({
            masterBill: ['',
                Validators.compose([
                    Validators.required
                ])],
            hbOfladingNo: ['',
                Validators.compose([
                    Validators.required
                ])],
            hbOfladingType: ['',
                Validators.compose([
                    Validators.required
                ])],
            finalDestination: [

            ],
            placeofReceipt: ['',
                Validators.compose([
                    Validators.required
                ])],
            feederVessel1: ['',
                Validators.compose([
                    Validators.required
                ])],
            feederVessel2: ['',
                Validators.compose([
                    Validators.required
                ])],
            arrivalVessel: [
            ],
            arrivalVoyage: [
            ],
            singledater: [
            ],
            documentNo: [],
            warehousecbo: [],
            referenceNo: [],
            warehousenotice: [],
            shppingMark: [],
            remark: []




        });
    }

}
