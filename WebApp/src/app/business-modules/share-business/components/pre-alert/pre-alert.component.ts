import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { ShareBusinessAddAttachmentPopupComponent } from '../add-attachment/add-attachment.popup';

@Component({
    selector: 'share-pre-alert',
    templateUrl: './pre-alert.component.html'
})
export class ShareBusinessReAlertComponent extends AppList {
    @ViewChild(ShareBusinessAddAttachmentPopupComponent, { static: false }) attachmentPopup: ShareBusinessAddAttachmentPopupComponent;

    onAddFile(files: IShipmentAttachFile) {
        console.log(files);
    }

    showPopup() {
        this.attachmentPopup.files.forEach(element => {
            element.isChecked = false;
        });
        this.attachmentPopup.show();

    }

}

interface IShipmentAttachFile {
    id: string;
    name: string;
    thumb: string;
    url: string;
    folder: string;
    objectId: string;
    extension: string;
    userCreated: string;
    dateTimeCreated: string;
    fileName: string;
}
