import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { SeaLCLExportFormBookingNoteComponent } from '../components/form-booking-note-lcl-export.component';
import { InfoPopupComponent } from '@common';
import { csBookingNote } from '@models';

import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { DocumentationRepo } from '@repositories';
import { Router } from '@angular/router';

import { catchError } from 'rxjs/operators';
import _merge from 'lodash/merge';

@Component({
    selector: 'app-sea-lcl-export-booking-note-create',
    templateUrl: './create-booking-note-lcl-export.component.html'
})

export class SeaLCLExportBookingNoteCreateComponent extends AppForm implements OnInit {
    @ViewChild(SeaLCLExportFormBookingNoteComponent, { static: false }) formBookingNoteComponent: SeaLCLExportFormBookingNoteComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;

    constructor(
        protected _toastService: ToastrService,
        protected _documentRepo: DocumentationRepo,
        protected _router: Router,
    ) {
        super();
    }

    ngOnInit() { }

    onSaveBookingNote() {
        this.formBookingNoteComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        const modelAdd: csBookingNote = this.onSubmitData();
        this.saveBookingNote(modelAdd);
    }

    checkValidateForm(): boolean {
        this.setError(this.formBookingNoteComponent.paymentTerm);

        let valid: boolean = true;
        if (!this.formBookingNoteComponent.formGroup.valid
            || (!!this.formBookingNoteComponent.etd.value && !this.formBookingNoteComponent.etd.value.startDate)
            || (!!this.formBookingNoteComponent.dateOfStuffing.value && !this.formBookingNoteComponent.dateOfStuffing.value.startDate)) {
            valid = false;
        }
        return valid;
    }

    onSubmitData(): csBookingNote {
        const form: any = this.formBookingNoteComponent.formGroup.getRawValue();
        const formData = {
            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            etd: !!form.etd && !!form.etd.startDate ? formatDate(form.etd.startDate, 'yyyy-MM-dd', 'en') : null,
            closingTime: !!form.closingTime && !!form.closingTime.startDate ? formatDate(form.closingTime.startDate, 'yyyy-MM-ddTHH:mm', 'en') : null,
            dateOfStuffing: !!form.dateOfStuffing && !!form.dateOfStuffing.startDate ? formatDate(form.dateOfStuffing.startDate, 'yyyy-MM-dd', 'en') : null,

            paymentTerm: !!form.paymentTerm && !!form.paymentTerm.length ? form.paymentTerm[0].id : null,

            pol: form.pol,
            pod: form.pod,
            shipperId: form.shipperId,
            consigneeId: form.consigneeId,
        };
        const bookingNote: csBookingNote = new csBookingNote(Object.assign(_merge(form, formData)));

        return bookingNote;
    }

    saveBookingNote(model: csBookingNote) {
        this._documentRepo.createCsBookingNote(model)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (res.result.success) {
                        this._toastService.success(res.result.message);

                        this._router.navigate([`home/documentation/sea-lcl-export/${res.model.id}`]);
                    } else {
                        this._toastService.error("Opps", "Something getting error!");
                    }
                }
            );
    }

    gotoList() {
        this._router.navigate(["home/documentation/sea-lcl-export/booking-note"]);
    }

}