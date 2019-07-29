import { Component, ViewChild } from "@angular/core";
import { ConfirmPopupComponent, InfoPopupComponent } from "src/app/shared/common/popup";
import { AccoutingRepo } from "src/app/shared/repositories";
import { catchError, finalize } from "rxjs/operators";
import { AppList } from "src/app/app.list";
import { SOA } from "src/app/shared/models";
import { ToastrService } from "ngx-toastr";
import { HttpErrorResponse } from "@angular/common/http";


@Component({
    selector: 'app-statement-of-account',
    templateUrl: './statement-of-account.component.html',
    styleUrls: ['./statement-of-account.component.scss']
})
export class StatementOfAccountComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;

    SOAs: SOA[] = [];
    selectedSOA: SOA = null;
    messageDelete: string = '';

    constructor(
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService
    ) {
        super();

        this.requestList = this.getSOAs;
    }

    ngOnInit() {
        this.getSOAs();
    }

    onDeleteSOA(soaItem: SOA) {
        this.selectedSOA = new SOA(soaItem);
        this.messageDelete = `Do you want to delete SOA ${soaItem.soano} ? `;
        this.confirmPopup.show();
    }

    deleteSOA() {
        this._accoutingRepo.deleteSOA(this.selectedSOA.soano).pipe(
            catchError(this.catchError),
            finalize(() => { this.confirmPopup.hide(); })
        ).subscribe(
            (res: any) => {
                this._toastService.success(res.message, '', { positionClass: 'toast-bottom-right' });

                // * search SOA when success.
                this.getSOAs();
            },
            (errors: any) => {
                this.handleError(errors);
            },
            () => { }
        );
    }

    getSOAs(data: any = {}) {
        this.isLoading = true;
        this._accoutingRepo.getListSOA(this.page, this.pageSize, data)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; })
            ).subscribe(
                (res: any) => {
                    this.SOAs = (res.data || []).map((item: SOA) => new SOA(item));
                    this.totalItems = res.totalItems || 0;
                },
                (errors: any) => {
                    this.handleError(errors);
                 },
                () => { }

            );
    }

    handleError(errors: any) {
        let message: string = 'Has Error Please Check Again !';
        let title: string = '';
        if (errors instanceof HttpErrorResponse) {
            message = errors.message;
            title = errors.statusText;
        }
        this._toastService.error(message, title, { positionClass: 'toast-bottom-right' });
    }

}
