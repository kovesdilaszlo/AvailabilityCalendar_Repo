// Handles calendar UI interactions and modal form binding.
document.addEventListener("DOMContentLoaded", () => {
    const autoSubmitElements = document.querySelectorAll(".js-auto-submit");

    autoSubmitElements.forEach(element => {
        element.addEventListener("change", () => {
            const form = element.closest("form");
            if (!form) {
                return;
            }

            form.submit();
        });
    });

    const eventModal = document.getElementById("eventActionModal");

    if (eventModal) {
        eventModal.addEventListener("show.bs.modal", event => {
            const trigger = event.relatedTarget;
            if (!trigger) {
                return;
            }

            const eventId = trigger.getAttribute("data-event-id") ?? "";
            const eventTitle = trigger.getAttribute("data-event-title") ?? "";
            const eventStart = trigger.getAttribute("data-event-start") ?? "";
            const eventEnd = trigger.getAttribute("data-event-end") ?? "";

            const summary = document.getElementById("eventModalSummary");
            const editEventId = document.getElementById("editEventId");
            const editEventTitle = document.getElementById("editEventTitle");
            const editEventStart = document.getElementById("editEventStart");
            const editEventEnd = document.getElementById("editEventEnd");
            const deleteEventId = document.getElementById("deleteEventId");

            if (summary) {
                summary.textContent = `${eventTitle} (${eventStart.replace("T", " ")} - ${eventEnd.replace("T", " ")})`;
            }

            if (editEventId) {
                editEventId.value = eventId;
            }

            if (editEventTitle) {
                editEventTitle.value = eventTitle;
            }

            if (editEventStart) {
                editEventStart.value = eventStart;
            }

            if (editEventEnd) {
                editEventEnd.value = eventEnd;
            }

            if (deleteEventId) {
                deleteEventId.value = eventId;
            }
        });
    }

    const freeSlotModal = document.getElementById("freeSlotModal");

    if (freeSlotModal) {
        freeSlotModal.addEventListener("show.bs.modal", event => {
            const trigger = event.relatedTarget;
            if (!trigger) {
                return;
            }

            const slotStart = trigger.getAttribute("data-slot-start") ?? "";
            const slotEnd = trigger.getAttribute("data-slot-end") ?? "";

            const summary = document.getElementById("freeSlotModalSummary");
            const startInput = document.getElementById("freeSlotStart");
            const endInput = document.getElementById("freeSlotEnd");

            if (summary) {
                summary.textContent = `${slotStart.replace("T", " ")} - ${slotEnd.replace("T", " ")}`;
            }

            if (startInput) {
                startInput.value = slotStart;
            }

            if (endInput) {
                endInput.value = slotEnd;
            }
        });
    }
});