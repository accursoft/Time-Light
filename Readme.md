Time Light
==========

Time Light keeps track of how much time you have spent on various projects.

The ledger is an XML file, containing a tree of projects and sub-projects.

Each element (except the root) is named with a name attribute. Nodes contain leaves and other nodes. Leaves contain the time spent on the leaf, while nodes have a total attribute.

The program's GUI is a timer icon in the notification area.

Right-clicking on the icon will bring up a menu of the project tree. Clicking on an element will select it as the active item, and begin timing. If a different element was active, it will be stopped.

The timer is green when timing, otherwise red. Clicking the timer icon will stop/start.

Each time the timer is stopped, the ledger totals are updated, including all ancestors of the active item.

If a node has a billed attribute, an unbilled attribute will be set to total hours - billed.

The ledger will be automatically reloaded if it is updated while the program is running.

Logging can be configured in the settings file. If the event log is used, the program must be run once as administrator to create the event source, or enter the following line from an elevated command prompt:

eventcreate /id 1 /l application /t information /so "Time Light" /d "create event source"

The settings file specifies the path to the ledger relative to the user's documents folder, and the program used for viewing/editing.

Example ledger
--------------

    <node>
      <node name="Colors">
        <leaf name="Orange" />
        <node name="Primary">
          <leaf name="Red" />
          <leaf name="Green" />
        </node>
      </node>
      <leaf name="Shapes" />
    </node>