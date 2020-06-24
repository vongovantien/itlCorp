import { Component, OnInit, ViewChild } from "@angular/core";
import { AppForm } from "src/app/app.form";
import { UnlockRequestInfoDeniedCommentPopupComponent } from "../popup/info-denied-comment/info-denied-comment.popup";

@Component({
    selector: 'process-approve-unlock-request',
    templateUrl: './process-approve-unlock-request.component.html',
    styleUrls: ['./process-approve-unlock-request.component.scss']
})

export class UnlockRequestProcessApproveComponent extends AppForm implements OnInit {
    @ViewChild(UnlockRequestInfoDeniedCommentPopupComponent, { static: false }) infoDeniedPopup: UnlockRequestInfoDeniedCommentPopupComponent;
    constructor(
    ) {
        super();
    }

    ngOnInit() {
    }

    showInfoDenied() {
        this.infoDeniedPopup.getDeniedComment("45514284-3D9A-4947-B289-3840FA79529C");
        this.infoDeniedPopup.show();

    }
}