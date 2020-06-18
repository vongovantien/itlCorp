import { Component } from "@angular/core";
import { PopupBase } from "src/app/popup.base";

@Component({
    selector: 'info-denied-comment-popup',
    templateUrl: './info-denied-comment.popup.html'
})
export class UnlockRequestInfoDeniedCommentPopupComponent extends PopupBase {
    infoDenieds: any[] = [];

    constructor(
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'No', field: 'no', sortable: true },
            { title: 'Name & Deny Time', field: 'name', sortable: true },
            { title: 'Level Approval', field: 'levelApprove', sortable: true },
            { title: 'Comment', field: 'comment', sortable: true },
        ];
    }

    closePopup() {
        this.hide();
    }
}