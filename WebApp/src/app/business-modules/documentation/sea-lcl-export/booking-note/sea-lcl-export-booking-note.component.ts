import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { SortService } from '@services';
import { Router } from '@angular/router';
import { DocumentationRepo } from '@repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { ConfirmPopupComponent } from '@common';
@Component({
    selector: 'app-sea-lcl-export-booking-note',
    templateUrl: './sea-lcl-export-booking-note.component.html'
})

export class SeaLCLExportBookingNoteComponent extends AppList implements OnInit {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    headers: CommonInterface.IHeaderTable[];

    criteria: any = {};
    bookingNotes: any = [];

    idBookingNote: string = '';

    _fromDate: Date = this.createMoment().startOf('month').toDate();
    _toDate: Date = this.createMoment().endOf('month').toDate();
    sortLocal(sort: string): void {
        this.bookingNotes = this._sortService.sort(this.bookingNotes, sort, this.order);
    }

    constructor(private _progressService: NgProgress,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _router: Router) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.searchBookingNote;
        this.requestSort = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Booking No', field: 'bookingNo', sortable: true, width: 100 },
            { title: 'Shipper', field: 'shipperName', sortable: true },
            { title: 'Consignee', field: 'consigneeName', sortable: true },
            { title: 'ETD', field: 'etd', sortable: true },
            { title: 'ETA', field: 'eta', sortable: true },
            { title: 'POL', field: 'polName', sortable: true },
            { title: 'POD', field: 'podName', sortable: true },
            { title: 'GW', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Creator', field: 'creatorName', sortable: true },
            { title: 'Create Date', field: 'createDate', sortable: true },
        ];

        this.dataSearch = {
            type: 'All'
        };
        this.criteria.fromDate = this._fromDate;
        this.criteria.toDate = this._toDate;
        this.searchBookingNote();
    }

    searchBookingNote() {
        this.isLoading = true;
        this._progressRef.start();
        this._documentRepo.getBookingNoteSeaLCLExport(this.page, this.pageSize, Object.assign({}, this.criteria))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((data: any) => {
                    if (data.data != null) {
                        return {
                            data: data.data,
                            totalItems: data.totalItems,
                        };
                    }
                    return {
                        data: null,
                        totalItems: 0,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.bookingNotes = res.data;
                    console.log(this.bookingNotes);
                },
            );
    }

    onSearchBookingNote(dataSearch: any) {
        this.dataSearch = dataSearch;
        this.page = 1;
        this.criteria = {};
        if (this.dataSearch.type === 'All') {
            this.criteria.all = this.dataSearch.keyword;
        } else {
            this.criteria.all = null;
        }
        if (this.dataSearch.type === 'creatorName') {
            this.criteria.creatorName = this.dataSearch.keyword;
        }
        if (this.dataSearch.type === 'bookingNo') {
            this.criteria.bookingNo = this.dataSearch.keyword;
        }
        if (this.dataSearch.type === 'shipperName') {
            this.criteria.shipperName = this.dataSearch.keyword;
        }
        if (this.dataSearch.type === 'consigneeName') {
            this.criteria.consigneeName = this.dataSearch.keyword;
        }
        if (this.dataSearch.type === 'polName') {
            this.criteria.polName = this.dataSearch.keyword;
        }
        if (this.dataSearch.type === 'podName') {
            this.criteria.podName = this.dataSearch.keyword;
        }
        this.criteria.fromDate = this.dataSearch.fromDate;
        this.criteria.toDate = this.dataSearch.toDate;
        this.searchBookingNote();
    }

    showDeletePopup(id: string) {
        this.confirmDeletePopup.show();
        this.idBookingNote = id;
    }

    onDeleteBookingNote() {
        this.confirmDeletePopup.hide();
        this.deleteBookingNote(this.idBookingNote);
    }

    deleteBookingNote(id: string) {
        this.isLoading = true;
        this._progressRef.start();
        this._documentRepo.deleteBookingNoteSeaLCLExport(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.searchBookingNote();
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }

    gotoDetail(id: string) {
        this._router.navigate([`/home/documentation/sea-lcl-export/booking-note/${id}`]);
    }

}
