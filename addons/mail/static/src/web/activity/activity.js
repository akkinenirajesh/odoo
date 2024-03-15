/* @odoo-module */

import { Component, useState, onMounted, onWillUnmount } from "@odoo/owl";
import { useService } from "@web/core/utils/hooks";
import { sprintf } from "@web/core/utils/strings";
import { usePopover } from "@web/core/popover/popover_hook";
import { FileUploader } from "@web/views/fields/file_handler";
import { browser } from "@web/core/browser/browser";
import { deserializeDateTime } from "@web/core/l10n/dates";

import { ActivityMailTemplate } from "@mail/web/activity/activity_mail_template";
import { ActivityMarkAsDone } from "@mail/web/activity/activity_markasdone_popover";
import { computeDelay, getMsToTomorrow } from "@mail/utils/dates";
import { useAttachmentUploader } from "@mail/attachments/attachment_uploader_hook";

import { _t } from "@web/core/l10n/translation";
import { useMessaging } from "@mail/core/messaging_hook";

/**
 * @typedef {Object} Props
 * @property {import("./activity_model").Activity} data
 * @property {function} [onUpdate]
 * @property {function} reloadParentView
 * @extends {Component<Props, Env>}
 */
export class Activity extends Component {
    static components = { ActivityMailTemplate, FileUploader };
    static props = ["data", "onUpdate?", "reloadParentView"];
    static defaultProps = { onUpdate: () => {} };
    static template = "mail.Activity";

    /** @type {function} */
    closePopover;

    setup() {
        this.messaging = useMessaging();
        /** @type {import("@mail/web/activity/activity_service").ActivityService} */
        this.activityService = useService("mail.activity");
        /** @type {import("@mail/core/thread_service").ThreadService} */
        this.threadService = useService("mail.thread");
        this.state = useState({ showDetails: false });
        this.popover = usePopover(ActivityMarkAsDone, { position: "right" });
        onMounted(() => {
            this.updateDelayAtNight();
        });
        onWillUnmount(() => browser.clearTimeout(this.updateDelayMidnightTimeout));
        this.attachmentUploader = useAttachmentUploader(this.thread);
    }

    get displayName() {
        if (this.props.data.summary) {
            return sprintf(_t("“%s”"), this.props.data.summary);
        }
        return this.props.data.display_name;
    }

    get displayCreateDate() {
        return deserializeDateTime(this.props.data.create_date).toLocaleString(
            luxon.DateTime.DATETIME_SHORT_WITH_SECONDS
        );
    }

    updateDelayAtNight() {
        browser.clearTimeout(this.updateDelayMidnightTimeout);
        this.updateDelayMidnightTimeout = browser.setTimeout(
            () => this.render(),
            getMsToTomorrow() + 100
        ); // Make sure there is no race condition
    }

    get delay() {
        return computeDelay(this.props.data.date_deadline);
    }

    get onUpdate() {
        return this.props.onUpdate?.();
    }

    toggleDetails() {
        this.state.showDetails = !this.state.showDetails;
    }

    async onClickMarkAsDone(ev) {
        if (this.popover.isOpen) {
            this.popover.close();
            return;
        }
        this.popover.open(ev.currentTarget, {
            activity: this.props.data,
            hasHeader: true,
            reload: this.onUpdate,
        });
    }

    async onFileUploaded(data) {
        const { id: attachmentId } = await this.attachmentUploader.uploadData(
            data,
            this.props.data
        );
        await this.activityService.markAsDone(this.props.data, [attachmentId]);
        this.onUpdate();
        await this.threadService.fetchNewMessages(this.thread);
    }

    async edit() {
        const { id, res_model, res_id } = this.props.data;
        await this.env.services["mail.activity"].schedule(res_model, res_id, id);
        this.onUpdate();
    }

    async unlink() {
        this.activityService.delete(this.props.data);
        await this.env.services.orm.unlink("mail.activity", [this.props.data.id]);
        this.onUpdate();
    }

    get thread() {
        return this.threadService.getThread(this.props.data.res_model, this.props.data.res_id);
    }
}
