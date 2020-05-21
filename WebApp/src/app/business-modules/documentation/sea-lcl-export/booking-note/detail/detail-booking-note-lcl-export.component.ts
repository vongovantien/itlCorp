import { Component, OnInit } from '@angular/core';
import { SeaLCLExportBookingNoteCreateComponent } from '../create/create-booking-note-lcl-export.component';
import { Router, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { DocumentationRepo } from '@repositories';
import { csBookingNote } from '@models';

import { map, tap, switchMap } from 'rxjs/operators';
import { of, combineLatest } from 'rxjs';
import isUUID from 'validator/lib/isUUID';

@Component({
    selector: 'app-sea-lcl-export-booking-note-detail',
    templateUrl: './detail-booking-note-lcl-export.component.html'
})

export class SeaLCLExportBookingNoteDetailComponent extends SeaLCLExportBookingNoteCreateComponent implements OnInit {

    bookingNoteId: string;
    ACTION: CommonType.ACTION_FORM | string = 'UPDATE';

    csBookingNote: csBookingNote = new csBookingNote();

    constructor(
        protected _router: Router,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        private _activedRoute: ActivatedRoute
    ) {
        super(_toastService, _documentRepo, _router)
    }

    ngOnInit() { }

    ngAfterViewInit() {
        combineLatest([
            this._activedRoute.params,
            this._activedRoute.queryParams
        ]).pipe(
            map(([params, qParams]) => ({ ...params, ...qParams })),
            tap((param: any) => {
                this.bookingNoteId = !!param.bookingNoteId ? param.bookingNoteId : '';
                if (param.action) {
                    this.ACTION = param.action.toUpperCase();
                } else {
                    this.ACTION = null;
                }

                // this._cd.detectChanges();
            }),
            switchMap(() => of(this.bookingNoteId)),
        ).subscribe(
            (bookingNoteId: string) => {
                if (isUUID(bookingNoteId)) {
                    this.getDetailBookingNote(bookingNoteId);
                } else {
                    this.gotoList();
                }
            }
        );
    }

    getDetailBookingNote(bookingNoteId: string) {
        this._documentRepo.getDetailCsBookingNote(bookingNoteId)
            .subscribe(
                (res: csBookingNote) => {
                    this.csBookingNote = new csBookingNote(res);
                    console.log(res);
                }
            );
    }

    gotoList() {
        this._router.navigate(["home/documentation/sea-lcl-export/booking-note"]);

    }
}