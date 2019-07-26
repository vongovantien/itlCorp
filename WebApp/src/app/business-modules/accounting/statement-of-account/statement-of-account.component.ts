import { Component, OnInit, ViewChild } from "@angular/core";
import { ConfirmPopupComponent, InfoPopupComponent } from "src/app/shared/common/popup";
import { AccoutingRepo } from "src/app/shared/repositories";
import { catchError } from "rxjs/operators";
import { AppList } from "src/app/app.list";
import { SOA } from "src/app/shared/models";


@Component({
    selector: 'app-statement-of-account',
    templateUrl: './statement-of-account.component.html',
    styleUrls: ['./statement-of-account.component.scss']
})
export class StatementOfAccountComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    
    SOAs: SOA[] = [];

    constructor(
        private _accoutingRepo: AccoutingRepo
    ) {
        super();

        this.requestList = this.getSOAs;
    }

    ngOnInit() {
        this.getSOAs();
    }

    onDeleteSOA(soaItem: SOA) {
        console.log(soaItem);
        // this.confirmPopup.show();
        // this.infoPopup.show();

    }

    getSOAs() {
        this._accoutingRepo.getListSOA(this.page, this.pageSize , {})
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.SOAs = (res.data || []).map( (item: SOA) => new SOA(item));
                    this.totalItems = res.totalItems || 0;
                    console.log(this.SOAs, this.totalItems);
                 },
                (errors: any) => { },
                () => { }

            );
    }

}
