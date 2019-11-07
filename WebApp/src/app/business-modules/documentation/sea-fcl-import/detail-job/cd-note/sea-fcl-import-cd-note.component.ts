import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { CdNoteAddPopupComponent } from '../../components/popup/add-cd-note/add-cd-note.popup';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError, map, finalize } from 'rxjs/operators';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { CdNoteDetailPopupComponent } from '../../components/popup/detail-cd-note/detail-cd-note.popup';

@Component({
    selector: 'sea-fcl-import-cd-note',
    templateUrl: './sea-fcl-import-cd-note.component.html',
})
export class SeaFCLImportCDNoteComponent extends AppList {
    @ViewChild(CdNoteAddPopupComponent, { static: false }) cdNoteAddPopupComponent: CdNoteAddPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteCdNotePopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteCdNotePopup: InfoPopupComponent;
    @ViewChild(CdNoteDetailPopupComponent, { static: false }) cdNoteDetailPopupComponent: CdNoteDetailPopupComponent;

    headers: CommonInterface.IHeaderTable[];
    idMasterBill: string = '11033e5a-01a6-400a-8798-5d23ecf26a4d';
    cdNoteGroups: any[] = [];
    deleteMessage: string = '';
    selectedCdNoteId: string = '';

    constructor(
        private _documentationRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _progressService: NgProgress,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Note No', field: 'code', sortable: true },
            { title: 'Charges Count', field: 'total_charge', sortable: true },
            { title: 'Balance Amount', field: 'total', sortable: true },
            { title: 'Creator', field: 'userCreated', sortable: true },
            { title: 'Create Date', field: 'datetimeCreated', sortable: true },
            { title: 'SOA', field: 'soaNo', sortable: true },
        ];

        this.getListCdNote(this.idMasterBill);
    }

    openPopupAdd() {
        //this.cdNoteAddPopupComponent.action = 'create';
        //this.cdNoteAddPopupComponent.advanceNo = this.advanceNo;
        this.cdNoteAddPopupComponent.show({ backdrop: 'static' });
    }

    openPopupDetail(){
        this.cdNoteDetailPopupComponent.show({ backdrop: 'static' });
    }

    getListCdNote(id: string) {
        //getListCdNoteByMasterBill
        this._documentationRepo.getListCDNoteByHouseBill(id)
            .pipe(
                catchError(this.catchError),

            ).subscribe(
                (res: any) => {
                    console.log('list cd note')
                    this.cdNoteGroups = res;
                    console.log(this.cdNoteGroups)
                },
            );
    }

    checkDeleteCdNote(id: string) {
        this._progressRef.start();
        this._documentationRepo.checkCdNoteAllowToDelete(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.selectedCdNoteId = id;
                        console.log(this.selectedCdNoteId)
                        this.deleteMessage = `All related information will be lost? Are you sure you want to delete this Credit/Debit Note?`;
                        this.confirmDeleteCdNotePopup.show();
                    } else {
                        this.canNotDeleteCdNotePopup.show();
                    }
                },
            );
    }

    onDeleteCdNote() {
        console.log('đang delete CDNote')
        console.log(this.selectedCdNoteId)
        this._progressRef.start();
        this._documentationRepo.deleteCdNote(this.selectedCdNoteId)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.confirmDeleteCdNotePopup.hide();
                })
            ).subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {
                        this._toastService.success(respone.message, 'Delete Success !');
                        console.log('id master bill ', this.idMasterBill);
                        this.getListCdNote(this.idMasterBill);
                    }
                },
            );
    }
}
